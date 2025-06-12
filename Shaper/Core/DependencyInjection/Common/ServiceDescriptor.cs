using System;
using Shaper.Core.DependencyInjection.Enum;

namespace Shaper.Core.DependencyInjection.Common
{
    // Service Descriptor
    /// <summary>
    /// Describes a service registration within the DI container.
    /// </summary>
    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object ImplementationInstance { get; }
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// For services registered with a specific implementation type (Transient, Scoped)
        /// </summary>
        /// <param name="serviceType">A service </param>
        /// <param name="implementationType">A implementation of service</param>
        /// <param name="lifetime">A lifetime of an object</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
        }

        /// <summary>
        /// For services registered with an existing instance (Singleton)
        /// </summary>
        /// <param name="serviceType">A service </param>
        /// <param name="implementationType">A implementation of service</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceDescriptor(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationInstance = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            ImplementationType = implementationType;
            Lifetime = ServiceLifetime.Singleton;
        }
        
        public ServiceDescriptor(Type serviceType, object implInstance)
        {
            ThrowNullArg(nameof(serviceType));
            ThrowNullArg(nameof(implInstance));
            
            ServiceType = serviceType ;
            ImplementationInstance = implInstance;
            Lifetime = ServiceLifetime.Singleton;
        }

        private static void ThrowNullArg(object param)
        {
            if(param is null) throw new ArgumentNullException(nameof(param));
        }
    }
}
