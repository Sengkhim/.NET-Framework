using System;
using System.Threading.Tasks;
using ShaperUtilities.Core.Abstraction.Resiliency.Builder;

namespace ShaperUtilities.Core.Abstraction.Resiliency.Base;

public abstract class Policy
{
    public abstract void Execute(Action action);
    public abstract T Execute<T>(Func<T> action);
    public abstract Task ExecuteAsync(Func<Task> action);
    public abstract Task<T> ExecuteAsync<T>(Func<Task<T>> action);

    public static RetryPolicyBuilder Handle<TException>() where TException : Exception
    {
        return new RetryPolicyBuilder(typeof(TException));
    }
}
