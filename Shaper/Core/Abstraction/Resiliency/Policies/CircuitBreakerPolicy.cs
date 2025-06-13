using System;
using System.Threading.Tasks;
using Shaper.Core.Abstraction.Resiliency.Base;

namespace Shaper.Core.Abstraction.Resiliency.Policies;

public class CircuitBreakerPolicy(int failureThreshold, TimeSpan openDuration) : Policy
{
    private int _failureCount;
    private DateTime _lastFailureTime = DateTime.MinValue;

    private bool IsOpen() =>_failureCount >= failureThreshold 
        && DateTime.UtcNow - _lastFailureTime < openDuration;

    private void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
    }

    private void Reset() => _failureCount = 0;

    private void TryExecute(Action action)
    {
        if (IsOpen())
            throw new InvalidOperationException("Circuit breaker is open.");

        try
        {
            action();
            Reset();
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }

    public override void Execute(Action action) => TryExecute(action);
    
    public override T Execute<T>(Func<T> action)
    {
        T result = default;
        TryExecute(() => result = action());
        return result;
    }

    public override async Task ExecuteAsync(Func<Task> action)
    {
        if (IsOpen())
            throw new InvalidOperationException("Circuit breaker is open.");

        try
        {
            await action();
            Reset();
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }

    public override async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (IsOpen())
            throw new InvalidOperationException("Circuit breaker is open.");

        try
        {
            var result = await action();
            Reset();
            return result;
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }
}
