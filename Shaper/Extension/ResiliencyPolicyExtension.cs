using System;
using System.Data.SqlClient;
using Shaper.Core.Abstraction.Resiliency.Base;
using Shaper.Core.Abstraction.Resiliency.Builder;
using Shaper.Core.Abstraction.Resiliency.Policies;

namespace Shaper.Extension;

public abstract class ResiliencyPolicy
{
    public static Policy Timeout(TimeSpan timeout) => new TimeoutPolicy(timeout);
    
    public static FallbackPolicyBuilder Fallback => new();
    
    public static Policy CircuitBreaker(int failureThreshold, TimeSpan openDuration)
        => new CircuitBreakerPolicy(failureThreshold, openDuration);
    
    public static RetryPolicy HandleRetry =>
        Policy
            .Handle<SqlException>()
            .WaitAndRetry(3, t => TimeSpan.FromSeconds(t))
            .Build();

}