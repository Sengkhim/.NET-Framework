namespace Shaper.Core.DependencyInjection.Enum;

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