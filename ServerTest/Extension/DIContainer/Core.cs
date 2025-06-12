using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServerTest.Extension.DIContainer
{
    // 1. Service Lifetime Enum
    /// <summary>
    /// Defines the lifetime of a service in the DI container.
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// A new instance of the service is created every time it is requested.
        /// </summary>
        Transient,

        /// <summary>
        /// A single instance of the service is created and reused throughout the application's lifetime.
        /// </summary>
        Singleton,

        /// <summary>
        /// A single instance of the service is created per scope (e.g., per HTTP request).
        /// </summary>
        Scoped
    }

    // 2. Service Descriptor
    /// <summary>
    /// Describes a service registration within the DI container.
    /// </summary>
    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object ImplementationInstance { get; }
        public ServiceLifetime Lifetime { get; }

        // For services registered with a specific implementation type (Transient, Scoped)
        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
        }

        // For services registered with an existing instance (Singleton)
        public ServiceDescriptor(Type serviceType, object implementationInstance)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
            Lifetime = ServiceLifetime.Singleton;
        }
    }

    // 3. IServiceCollection Interface
    /// <summary>
    /// Contract for a collection of service descriptors.
    /// Used to register services with the DI container.
    /// </summary>
    public interface IServiceCollection
    {
        IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : TService;
        IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : TService;
        IServiceCollection AddSingleton<TService>(TService instance);
        IServiceCollection AddScoped<TService, TImplementation>() where TImplementation : TService;
        /// <summary>
        /// This method will build the IServiceProvider from the registered services.
        /// </summary>
        IServiceProvider BuildServiceProvider();
    }

    // 3. ServiceCollection Implementation
    /// <summary>
    /// Implements IServiceCollection to manage service registrations.
    /// </summary>
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> _serviceDescriptors = new List<ServiceDescriptor>();

        public IServiceCollection AddTransient<TService, TImplementation>()
            where TImplementation : TService
        {
            _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
            return this;
        }

        public IServiceCollection AddSingleton<TService, TImplementation>()
            where TImplementation : TService
        {
            _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
            return this;
        }

        public IServiceCollection AddSingleton<TService>(TService instance)
        {
            _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), instance));
            return this;
        }

        public IServiceCollection AddScoped<TService, TImplementation>()
            where TImplementation : TService
        {
            _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
            return this;
        }

        public IServiceProvider BuildServiceProvider()
        {
            // The root ServiceProvider is responsible for creating and managing Singletons.
            // It will also create the IServiceScopeFactory for Scoped services.
            var serviceProvider = new ServiceProvider(_serviceDescriptors);
            serviceProvider.InitializeSingletons(); // Initialize singletons immediately
            return serviceProvider;
        }
    }

    // 4. IServiceProvider Interface
    /// <summary>
    /// Contract for a service provider that can resolve registered services.
    /// </summary>
    public interface IServiceProvider : IDisposable
    {
        /// <summary>
        /// Gets an instance of the service. Returns null if not found.
        /// </summary>
        TService GetService<TService>();
        object GetService(Type serviceType);

        /// <summary>
        /// Gets an instance of the service. Throws an exception if not found.
        /// </summary>
        TService GetRequiredService<TService>();
        object GetRequiredService(Type serviceType);
    }

    // 5. ServiceProvider Implementation
    /// <summary>
    /// The core DI container that resolves service instances.
    /// Handles Transient, Singleton, and provides IServiceScopeFactory for Scoped.
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly List<ServiceDescriptor> _serviceDescriptors;
        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, ServiceDescriptor> _descriptorMap;

        // For managing scoped instances within a specific scope (e.g., HTTP request)
        private ConcurrentDictionary<Type, object> _scopedInstances;
        private bool _isRootProvider; // True if this is the root provider, false if it's a scoped provider

        // Constructor for the root ServiceProvider
        public ServiceProvider(List<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
            _descriptorMap = new ConcurrentDictionary<Type, ServiceDescriptor>(
                serviceDescriptors.ToDictionary(sd => sd.ServiceType)
            );
            _isRootProvider = true;

            // Register IServiceScopeFactory itself as a singleton
            // This allows us to create new scopes from the root provider.
            // Note: This requires a bit of a self-referential trick, as ServiceProvider
            // needs to be fully constructed before we can register the factory.
            // This is handled by a late initialization for the factory if not pre-registered.
            if (!_descriptorMap.ContainsKey(typeof(IServiceScopeFactory)))
            {
                _serviceDescriptors.Add(new ServiceDescriptor(typeof(IServiceScopeFactory), new ServiceScopeFactory(this)));
                 _descriptorMap = new ConcurrentDictionary<Type, ServiceDescriptor>( // Rebuild map after adding
                    _serviceDescriptors.ToDictionary(sd => sd.ServiceType)
                );
            }
        }

        // Constructor for a scoped ServiceProvider
        private ServiceProvider(List<ServiceDescriptor> serviceDescriptors, ConcurrentDictionary<Type, object> singletonInstances)
        {
            _serviceDescriptors = serviceDescriptors;
            _singletonInstances = singletonInstances; // Share singleton instances from the root provider
            _descriptorMap = new ConcurrentDictionary<Type, ServiceDescriptor>(
                serviceDescriptors.ToDictionary(sd => sd.ServiceType)
            );
            _scopedInstances = new ConcurrentDictionary<Type, object>(); // New scoped instances for this scope
            _isRootProvider = false;
        }

        /// <summary>
        /// Initializes singleton instances specified with an existing instance.
        /// Called by the ServiceCollection after building the root ServiceProvider.
        /// </summary>
        internal void InitializeSingletons()
        {
            foreach (var descriptor in _serviceDescriptors.Where(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ImplementationInstance != null))
            {
                _singletonInstances.TryAdd(descriptor.ServiceType, descriptor.ImplementationInstance);
            }
        }

        public TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        private bool CheckServiceType(Type serviceType)
            => !serviceType.IsInterface && !serviceType.IsAbstract;

        public object GetService(Type serviceType)
        {
            if (!_descriptorMap.TryGetValue(serviceType, out var descriptor))
            {
                return CreateInstance(serviceType);
                // return CheckServiceType(serviceType) ? CreateInstance(serviceType) : null;
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Transient:
                    return CreateInstance(descriptor.ImplementationType);
                case ServiceLifetime.Singleton:
                    return _singletonInstances.GetOrAdd(descriptor.ServiceType, _ => CreateInstance(descriptor.ImplementationType));
                case ServiceLifetime.Scoped:
                    if (_isRootProvider)
                    {
                        throw new InvalidOperationException($"Cannot resolve scoped service '{serviceType.Name}' from root service provider. Use IServiceScopeFactory.");
                    }
                    return _scopedInstances.GetOrAdd(descriptor.ServiceType, _ => CreateInstance(descriptor.ImplementationType));
                default:
                    throw new NotSupportedException($"Service lifetime '{descriptor.Lifetime}' not supported.");
            }
        }

        public TService GetRequiredService<TService>()
        {
            return (TService)GetRequiredService(typeof(TService));
        }

        public object GetRequiredService(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service == null)
                throw new InvalidOperationException($"Service of type '{serviceType.Name}' could not be resolved.");
            return service;
        }

        /// <summary>
        /// Creates an instance of the given type, resolving its constructor dependencies recursively.
        /// </summary>
        private object CreateInstance(Type implementationType)
        {
            // Get the constructor with the most parameters (greedy constructor injection)
            ConstructorInfo constructor = implementationType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
            {
                throw new InvalidOperationException($"No public constructor found for type '{implementationType.Name}'.");
            }

            var parameters = constructor.GetParameters();
            var parameterInstances = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterInstances[i] = GetRequiredService(parameters[i].ParameterType);
            }

            return constructor.Invoke(parameterInstances);
        }

        /// <summary>
        /// Disposes of disposable scoped instances.
        /// </summary>
        public void Dispose()
        {
            // Only scoped providers dispose of their own scoped instances.
            if (!_isRootProvider && _scopedInstances != null)
            {
                foreach (var instance in _scopedInstances.Values)
                {
                    (instance as IDisposable)?.Dispose();
                }
                _scopedInstances.Clear();
            }
            // Root provider can dispose singletons if they are IDisposable.
            // This is typically done at application shutdown.
            if (_isRootProvider && _singletonInstances != null)
            {
                foreach (var instance in _singletonInstances.Values)
                {
                    (instance as IDisposable)?.Dispose();
                }
                _singletonInstances.Clear();
            }
        }

        /// <summary>
        /// Internal method used by ServiceScopeFactory to create a new scoped provider.
        /// </summary>
        internal IServiceProvider CreateScopedServiceProvider()
        {
            return new ServiceProvider(_serviceDescriptors, _singletonInstances);
        }
    }

    // 6. IServiceScopeFactory and IServiceScope
    /// <summary>
    /// Contract for a factory that creates new service scopes.
    /// </summary>
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }

    /// <summary>
    /// Contract for a service scope, providing access to a scoped service provider.
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }

    /// <summary>
    /// Factory for creating new service scopes.
    /// </summary>
    public class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly ServiceProvider _rootServiceProvider;

        public ServiceScopeFactory(ServiceProvider rootServiceProvider)
        {
            _rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(_rootServiceProvider.CreateScopedServiceProvider());
        }
    }

    /// <summary>
    /// Represents a service scope, managing its own IServiceProvider and disposing it.
    /// </summary>
    public class ServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Dispose()
        {
            ServiceProvider.Dispose(); // Dispose the scoped ServiceProvider
        }
    }

    // Helper for ServiceCollection extensions (similar to .NET Core)
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddTransient<TService, TService>();
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddSingleton<TService, TService>();
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddScoped<TService, TService>();
        }
    }
}
