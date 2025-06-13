using System;
using System.Threading.Tasks;
using Shaper.Core.Abstraction.Resiliency.Base;

namespace Shaper.Core.Abstraction.Resiliency.Policies;

public class TimeoutPolicy(TimeSpan timeout) : Policy
{
    public override void Execute(Action action)
    {
        var task = Task.Run(action);
        
        if (!task.Wait(timeout)) throw new TimeoutException("Operation timed out.");
    }

    public override T Execute<T>(Func<T> action)
    {
        var task = Task.Run(action);
        if (task.Wait(timeout)) return task.Result;
        throw new TimeoutException("Operation timed out.");
    }

    public override async Task ExecuteAsync(Func<Task> action)
    {
        var task = action();
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            await task;
        else
            throw new TimeoutException("Operation timed out.");
    }

    public override async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        var task = action();
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            return await task;
        throw new TimeoutException("Operation timed out.");
    }
}
