using System;
using System.Threading;
using System.Threading.Tasks;
using Shaper.Core.Abstraction.Resiliency.Base;

namespace Shaper.Core.Abstraction.Resiliency.Policies;

public class RetryPolicy(
    Type exceptionType,
    int retryCount,
    Func<int, TimeSpan> sleepDurationProvider): Policy
{
    public override void Execute(Action action)
        => Execute<object>(() => { action(); return null; });

    public override T Execute<T>(Func<T> action)
    {
        for (var attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex) when (exceptionType.IsInstanceOfType(ex))
            {
                if (attempt == retryCount) throw;

                Thread.Sleep(sleepDurationProvider(attempt));
            }
        }

        throw new InvalidOperationException("Retry attempts exhausted.");
    }

    
    public override async Task ExecuteAsync(Func<Task> action)
        => await ExecuteAsync<object>(async () => { await action(); return null; });

    
    public override async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        for (var attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (exceptionType.IsInstanceOfType(ex))
            {
                if (attempt == retryCount)
                    throw;

                await Task.Delay(sleepDurationProvider(attempt));
            }
        }

        throw new InvalidOperationException("Retry attempts exhausted.");
    }
}