using System;
using ShaperUtilities.Core.DependencyInjection.Service;
using IServiceProvider = ShaperUtilities.Core.DependencyInjection.Service.IServiceProvider;

namespace ShaperUtilities.Core.DependencyInjection.Implement;

/// <summary>
/// Represents a service scope, managing its own IServiceProvider and disposing it.
/// </summary>
public class ServiceScope(IServiceProvider serviceProvider) : IServiceScope
{
    public IServiceProvider ServiceProvider { get; } 
        = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Dispose the scoped ServiceProvider
    /// </summary>
    public void Dispose() => ServiceProvider.Dispose(); 
}