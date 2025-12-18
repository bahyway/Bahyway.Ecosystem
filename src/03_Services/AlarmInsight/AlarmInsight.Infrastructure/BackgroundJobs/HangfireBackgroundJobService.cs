using BahyWay.SharedKernel.Application.Abstractions;
using Hangfire;
using System.Linq.Expressions;

namespace AlarmInsight.Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    // --- 1. Enqueue (Fire-and-Forget) ---
    public string Enqueue(Expression<Action> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    // --- 2. Schedule (Delayed by DateTimeOffset) ---
    public string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    // --- 3. Schedule (Delayed by TimeSpan) - THIS WAS THE MISSING PART ---
    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    // --- 4. Recurring Jobs ---
    // Note: If your interface calls this 'Recur', change the name below to 'Recur'.
    // If it calls it 'AddOrUpdateRecurringJob', keep it as is.
    // Based on standard patterns, I will provide the most common implementation:

    public void Recur(string jobId, Expression<Action> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    public void Recur(string jobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    // Also adding the verbose name just in case the interface uses that
    public void AddOrUpdateRecurringJob(string recurringJobId, Expression<Action> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    public void AddOrUpdateRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    // --- 5. Deletion ---
    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }

    public void RemoveRecurringJob(string recurringJobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
    }

    // --- 6. Continuations ---
    public string ContinueWith(string parentJobId, Expression<Action> methodCall)
    {
        return BackgroundJob.ContinueJobWith(parentJobId, methodCall);
    }

}