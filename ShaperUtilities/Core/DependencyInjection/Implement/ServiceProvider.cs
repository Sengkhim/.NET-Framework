﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ShaperUtilities.Core.DependencyInjection.Common;
using ShaperUtilities.Core.DependencyInjection.Enum;
using ShaperUtilities.Core.DependencyInjection.Service;
using IServiceProvider = ShaperUtilities.Core.DependencyInjection.Service.IServiceProvider;

namespace ShaperUtilities.Core.DependencyInjection.Implement;

    /// <summary>
    /// The core DI container that resolves service instances.
    /// Handles Transient, Singleton, and provides IServiceScopeFactory for Scoped.
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly List<ServiceDescriptor> _serviceDescriptors;
        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ();
        private readonly ConcurrentDictionary<Type, ServiceDescriptor> _descriptorMap;

        // For managing scoped instances within a specific scope (e.g., HTTP request)
        private readonly ConcurrentDictionary<Type, object> _scopedInstances;
        
        // True if this is the root provider, false if it's a scoped provider
        private readonly bool _isRootProvider;
        
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
            if (_descriptorMap.ContainsKey(typeof(IServiceScopeFactory))) return;
            
            _serviceDescriptors.Add(new ServiceDescriptor(typeof(IServiceScopeFactory), new ServiceScopeFactory(this)));
            _descriptorMap = new ConcurrentDictionary<Type, ServiceDescriptor>( // Rebuild map after adding
                _serviceDescriptors.ToDictionary(sd => sd.ServiceType)
            );
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
            var descriptors = _serviceDescriptors
                .Where(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ImplementationInstance != null);
            
            foreach (var descriptor in descriptors)
                _singletonInstances.TryAdd(descriptor.ServiceType, descriptor.ImplementationInstance);
        }

        public TService GetService<TService>() => (TService)GetService(typeof(TService));
        

        public object GetService(Type serviceType)
        {
            if (!_descriptorMap.TryGetValue(serviceType, out var descriptor))
                return CreateInstance(serviceType);

            return descriptor.Lifetime switch
            {
                ServiceLifetime.Transient => CreateInstance(descriptor.ImplementationType),
                ServiceLifetime.Singleton => SingletonInstance(serviceType, descriptor),
                ServiceLifetime.Scoped => ScopeInstance(serviceType, descriptor),
                _ => throw new NotSupportedException($"Service lifetime '{descriptor.Lifetime}' not supported.")
            };
        }
        
        private object TryCreateInstance(Type serviceType, ServiceDescriptor descriptor)
            => _scopedInstances.GetOrAdd(serviceType, _ => CreateInstance(descriptor.ImplementationType));

        private object ScopeInstance(Type serviceType, ServiceDescriptor descriptor)
        {
            if (_isRootProvider)
                throw new InvalidOperationException($"Cannot resolve scoped service '{serviceType.Name}' from root service provider. Use IServiceScopeFactory.");
            return TryCreateInstance(serviceType, descriptor);
        }

        private object SingletonInstance(Type serviceType, ServiceDescriptor descriptor)
            => TryCreateInstance(serviceType, descriptor);

        public TService GetRequiredService<TService>() => (TService)GetRequiredService(typeof(TService));

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
            var constructor = implementationType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
                throw new InvalidOperationException($"No public constructor found for type '{implementationType.Name}'.");

            var parameters = constructor.GetParameters();
            var parameterInstances = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                parameterInstances[i] = GetRequiredService(parameters[i].ParameterType);

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
            if (!_isRootProvider || _singletonInstances == null) return;
            
            foreach (var instance in _singletonInstances.Values)
                (instance as IDisposable)?.Dispose();
            
            _singletonInstances.Clear();
        }

        /// <summary>
        /// Internal method used by ServiceScopeFactory to create a new scoped provider.
        /// </summary>
        internal IServiceProvider CreateScopedServiceProvider()
            => new ServiceProvider(_serviceDescriptors, _singletonInstances);
    }
