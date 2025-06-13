using System;

namespace ShaperUtilities.Core.DependencyInjection.Service;

/// <summary>
/// Contract for a service scope, providing access to a scoped service provider.
/// </summary>
public interface IServiceScope : IDisposable
{
    IServiceProvider ServiceProvider { get; }
}