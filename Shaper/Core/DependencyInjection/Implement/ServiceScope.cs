using System;
using Shaper.Core.DependencyInjection.Service;
using IServiceProvider = Shaper.Core.DependencyInjection.Service.IServiceProvider;
using Service_IServiceProvider = Shaper.Core.DependencyInjection.Service.IServiceProvider;

namespace Shaper.Core.DependencyInjection.Implement;


/// <summary>
/// Represents a service scope, managing its own IServiceProvider and disposing it.
/// </summary>
public class ServiceScope(Service_IServiceProvider serviceProvider) : IServiceScope
{
    public Service_IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public void Dispose()
    {
        //Dispose the scoped ServiceProvider
        ServiceProvider.Dispose(); 
    }
}