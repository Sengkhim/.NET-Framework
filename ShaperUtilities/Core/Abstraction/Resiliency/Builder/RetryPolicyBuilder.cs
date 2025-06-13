using System;
using ShaperUtilities.Core.Abstraction.Resiliency.Policies;

namespace ShaperUtilities.Core.Abstraction.Resiliency.Builder;

public class RetryPolicyBuilder(Type exceptionType)
{
    private int _retryCount = 3;
    private Func<int, TimeSpan> _sleepDurationProvider = _ => TimeSpan.FromSeconds(2);

    public RetryPolicyBuilder WaitAndRetry(int retryCount, Func<int, TimeSpan> sleepDurationProvider)
    {
        _retryCount = retryCount;
        _sleepDurationProvider = sleepDurationProvider;
        return this;
    }

    public RetryPolicy Build()
        => new RetryPolicy(exceptionType, _retryCount, _sleepDurationProvider);
}