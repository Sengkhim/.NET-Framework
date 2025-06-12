using System;

namespace Shaper.Core.DependencyInjection.Service;

// IServiceCollection Interface
/// <summary>
/// Contract for a collection of service descriptors.
/// Used to register services with the DI container.
/// </summary>
public interface IServiceCollection
{
    IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : TService;
    IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : TService;
    IServiceCollection AddSingleton<TService>(Type instance);
    IServiceCollection AddSingleton<TService>(object instance);
    IServiceCollection AddScoped<TService, TImplementation>() where TImplementation : TService;
    /// <summary>
    /// This method will build the IServiceProvider from the registered services.
    /// </summary>
    IServiceProvider BuildServiceProvider();
}