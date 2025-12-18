using AlarmInsight.Application.Abstractions;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Queries.GetActiveAlarms;

public sealed class GetActiveAlarmsQueryHandler
    : IRequestHandler<GetActiveAlarmsQuery, Result<IEnumerable<AlarmSummaryDto>>>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IApplicationLogger<GetActiveAlarmsQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetActiveAlarmsQueryHandler(
        IAlarmRepository alarmRepository,
        IApplicationLogger<GetActiveAlarmsQueryHandler> logger,
        ICacheService cache)
    {
        _alarmRepository = alarmRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<AlarmSummaryDto>>> Handle(
        GetActiveAlarmsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching active alarms");

        // Try cache first
        var cachedAlarms = await _cache.GetAsync<IEnumerable<AlarmSummaryDto>>(
            CacheKeys.Alarms.AllActive(),
            cancellationToken);

        if (cachedAlarms is not null)
        {
            _logger.LogDebug("Active alarms found in cache");
            return Result.Success(cachedAlarms);
        }

        // Get from database
        var alarms = await _alarmRepository.GetActiveAlarmsAsync(cancellationToken);

        // Map to DTOs
        var dtos = alarms.Select(alarm => new AlarmSummaryDto
        {
            Id = alarm.Id,
            Source = alarm.Source,
            Severity = alarm.Severity.Name,
            Location = alarm.Location.Name,
            Status = alarm.Status.ToString(),
            OccurredAt = alarm.OccurredAt
        });

        // Cache it
        await _cache.SetAsync(
            CacheKeys.Alarms.AllActive(),
            dtos,
            CacheExpiration.Short,
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} active alarms", dtos.Count());

        return Result.Success(dtos);
    }
}