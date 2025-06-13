using System;
using System.Threading.Tasks;
using ShaperUtilities.Core.Abstraction.Resiliency.Base;

namespace ShaperUtilities.Core.Abstraction.Resiliency.Policies;

public class FallbackPolicy(
    Func<Exception, bool> shouldHandle,
    Action fallbackAction,
    Func<Task> fallbackActionAsync)
    : Policy
{
    public override void Execute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (shouldHandle(ex))
        {
            fallbackAction?.Invoke();
        }
    }

    public override T Execute<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex) when (shouldHandle(ex))
        {
            fallbackAction?.Invoke();
            return default;
        }
    }

    public override async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex) when (shouldHandle(ex))
        {
            if (fallbackActionAsync != null)
                await fallbackActionAsync();
        }
    }

    public override async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (shouldHandle(ex))
        {
            if (fallbackActionAsync != null)
                await fallbackActionAsync();
            return default;
        }
    }
}
