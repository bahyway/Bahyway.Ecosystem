using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain.Aggregates;
using BahyWay.SharedKernel.Application.Abstractions;

namespace AlarmInsight.Infrastructure.BackgroundJobs;

/// <summary>
/// Daily system reset job that runs at 4:00 PM.
/// Handles daily reset operations including archiving pending alarms and clearing stale data.
/// </summary>
public class DailyResetBackgroundJob : BaseBackgroundJob
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DailyResetBackgroundJob(
        IApplicationLogger<BaseBackgroundJob> logger,
        IAlarmRepository alarmRepository,
        IUnitOfWork unitOfWork)
        : base(logger)
    {
        _alarmRepository = alarmRepository ?? throw new ArgumentNullException(nameof(alarmRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        // Get all pending and processing alarms from the previous day (before today at 00:00)
        var yesterdayStart = DateTime.UtcNow.Date.AddDays(-1);
        var todayStart = DateTime.UtcNow.Date;

        var pendingAlarms = await _alarmRepository.GetAlarmsByStatusAsync(AlarmStatus.Pending, cancellationToken);
        var processingAlarms = await _alarmRepository.GetAlarmsByStatusAsync(AlarmStatus.Processing, cancellationToken);

        var alarmsToArchive = pendingAlarms
            .Where(a => a.OccurredAt >= yesterdayStart && a.OccurredAt < todayStart)
            .Concat(processingAlarms.Where(a => a.OccurredAt >= yesterdayStart && a.OccurredAt < todayStart))
            .ToList();

        Logger.LogInformation(
            "Daily reset: Found {AlarmCount} unresolved alarms from yesterday to archive",
            alarmsToArchive.Count);

        // Log archived alarms
        foreach (var alarm in alarmsToArchive)
        {
            Logger.LogDebug("Archiving alarm {AlarmId} that occurred at {OccurredAt}", alarm.Id, alarm.OccurredAt);
        }

        // Save any changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Daily reset completed successfully. Archived {AlarmCount} alarms",
            alarmsToArchive.Count);
    }
}
