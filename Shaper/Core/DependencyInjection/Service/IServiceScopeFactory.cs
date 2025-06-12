namespace Shaper.Core.DependencyInjection.Service;

// IServiceScopeFactory and IServiceScope
/// <summary>
/// Contract for a factory that creates new service scopes.
/// </summary>
public interface IServiceScopeFactory
{
    IServiceScope CreateScope();
}