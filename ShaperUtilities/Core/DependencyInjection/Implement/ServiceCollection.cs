using System;
using System.Collections.Generic;
using ShaperUtilities.Core.DependencyInjection.Common;
using ShaperUtilities.Core.DependencyInjection.Enum;
using ShaperUtilities.Core.DependencyInjection.Service;
using IServiceProvider = ShaperUtilities.Core.DependencyInjection.Service.IServiceProvider;

namespace ShaperUtilities.Core.DependencyInjection.Implement;

// ServiceCollection Implementation
/// <summary>
/// Implements IServiceCollection to manage service registrations.
/// </summary>
public class ServiceCollection : IServiceCollection
{
    private readonly List<ServiceDescriptor> _serviceDescriptors = [];

    public IServiceCollection AddTransient<TService, TImplementation>()
        where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService),
            typeof(TImplementation), ServiceLifetime.Transient));
        
        return this;
    }

    public IServiceCollection AddSingleton<TService, TImplementation>()
        where TImplementation : TService
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService),
            typeof(TImplementation), ServiceLifetime.Singleton));
        
        return this;
    }

    public IServiceCollection AddSingleton<TService>(Type instance)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), instance));
        return this;
    }

    public IServiceCollection AddSingleton<TService>(object instance)
    {
        _serviceDescriptors.Add(new ServiceDescriptor(typeof(TService), instance));
        return this;
    }

    public IServiceCollection AddScoped<TService, TImplementation>()
        where TImplementation : TService
    {
        _serviceDescriptors.Add(
            new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
        return this;
    }

    public IServiceProvider BuildServiceProvider()
    {
        var serviceProvider = new ServiceProvider(_serviceDescriptors);
        serviceProvider.InitializeSingletons();
        return serviceProvider;
    }
}