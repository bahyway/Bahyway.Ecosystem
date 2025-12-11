using System.Linq.Expressions;

namespace BahyWay.SharedKernel.Application.Abstractions;

/// <summary>
/// Background job service abstraction for async task processing.
/// REUSABLE: ✅ ALL PROJECTS
/// HEAVY USERS: ETLway (file processing), AlarmInsight (notifications), SmartForesight (model training)
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueues a fire-and-forget job (runs as soon as possible).
    /// </summary>
    string Enqueue(Expression<Action> methodCall);

    /// <summary>
    /// Enqueues a fire-and-forget async job.
    /// </summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Schedules a job to run after a delay.
    /// </summary>
    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedules an async job to run after a delay.
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Creates or updates a recurring job with cron schedule.
    /// </summary>
    void AddOrUpdateRecurringJob(
        string jobId,
        Expression<Action> methodCall,
        string cronExpression);

    /// <summary>
    /// Creates or updates a recurring async job.
    /// </summary>
    void AddOrUpdateRecurringJob(
        string jobId,
        Expression<Func<Task>> methodCall,
        string cronExpression);

    /// <summary>
    /// Removes a recurring job.
    /// </summary>
    void RemoveRecurringJob(string jobId);

    /// <summary>
    /// Deletes a job from queue.
    /// </summary>
    bool Delete(string jobId);
}

/// <summary>
/// Common cron expressions for scheduling.
/// </summary>
public static class CronExpressions
{
    public const string EveryMinute = "* * * * *";
    public const string Every5Minutes = "*/5 * * * *";
    public const string Every15Minutes = "*/15 * * * *";
    public const string Every30Minutes = "*/30 * * * *";
    public const string Hourly = "0 * * * *";
    public const string Daily = "0 0 * * *";
    public const string DailyAt2AM = "0 2 * * *";
    public const string Weekly = "0 0 * * 0";
    public const string Monthly = "0 0 1 * *";
    public const string Weekdays9AM = "0 9 * * 1-5";

    public static string DailyAtHour(int hour) => $"0 {hour} * * *";
    public static string EveryNMinutes(int minutes) => $"*/{minutes} * * * *";
}

/// <summary>
/// Base class for background jobs with automatic logging.
/// </summary>
public abstract class BaseBackgroundJob
{
    protected IApplicationLogger<BaseBackgroundJob> Logger { get; }

    protected BaseBackgroundJob(IApplicationLogger<BaseBackgroundJob> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the job with automatic error handling and logging.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var jobName = GetType().Name;
        
        Logger.LogInformation("Starting background job: {JobName}", jobName);
        var startTime = DateTime.UtcNow;

        try
        {
            await ExecuteInternalAsync(cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            Logger.LogInformation(
                "Background job {JobName} completed successfully in {Duration}ms",
                jobName,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            Logger.LogError(
                ex,
                "Background job {JobName} failed after {Duration}ms",
                jobName,
                duration.TotalMilliseconds);
            
            throw;
        }
    }

    /// <summary>
    /// Implement this method with your job logic.
    /// </summary>
    protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);
}
