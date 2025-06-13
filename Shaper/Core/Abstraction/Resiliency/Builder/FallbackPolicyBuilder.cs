using System;
using System.Threading.Tasks;
using Shaper.Core.Abstraction.Resiliency.Policies;

namespace Shaper.Core.Abstraction.Resiliency.Builder;

public class FallbackPolicyBuilder
{
    private Func<Exception, bool> _shouldHandle;
    private Action _fallbackAction;
    private Func<Task> _fallbackActionAsync;

    public FallbackPolicyBuilder Handle<TException>() where TException : Exception
    {
        _shouldHandle = ex => ex is TException;
        return this;
    }

    public FallbackPolicyBuilder Fallback(Action fallbackAction)
    {
        _fallbackAction = fallbackAction;
        return this;
    }

    public FallbackPolicyBuilder FallbackAsync(Func<Task> fallbackActionAsync)
    {
        _fallbackActionAsync = fallbackActionAsync;
        return this;
    }

    public FallbackPolicy Build()
        => new FallbackPolicy(_shouldHandle, _fallbackAction, _fallbackActionAsync);
}
