using System;

namespace ShaperUtilities.Core.DependencyInjection.Service;

// IServiceProvider Interface
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