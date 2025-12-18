using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Queries.GetAlarm;

public sealed class GetAlarmQueryHandler : IRequestHandler<GetAlarmQuery, Result<AlarmDto>>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IApplicationLogger<GetAlarmQueryHandler> _logger;
    private readonly ICacheService _cache;

    public GetAlarmQueryHandler(
        IAlarmRepository alarmRepository,
        IApplicationLogger<GetAlarmQueryHandler> logger,
        ICacheService cache)
    {
        _alarmRepository = alarmRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<AlarmDto>> Handle(GetAlarmQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching alarm: {AlarmId}", request.AlarmId);

        // Try cache first
        var cachedAlarm = await _cache.GetAsync<AlarmDto>(
            CacheKeys.Alarms.ById(request.AlarmId),
            cancellationToken);

        if (cachedAlarm is not null)
        {
            _logger.LogDebug("Alarm found in cache: {AlarmId}", request.AlarmId);
            return Result.Success(cachedAlarm);
        }

        // Get from database
        var alarm = await _alarmRepository.GetByIdAsync(request.AlarmId, cancellationToken);

        if (alarm is null)
        {
            _logger.LogWarning("Alarm not found: {AlarmId}", request.AlarmId);
            return Result.Failure<AlarmDto>(AlarmErrors.NotFound(request.AlarmId));
        }

        // Map to DTO
        var dto = new AlarmDto
        {
            Id = alarm.Id,
            Source = alarm.Source,
            Description = alarm.Description,
            Severity = alarm.Severity.Name,
            Location = alarm.Location.Name,
            Latitude = alarm.Location.Latitude,
            Longitude = alarm.Location.Longitude,
            Status = alarm.Status.ToString(),
            OccurredAt = alarm.OccurredAt,
            ProcessedAt = alarm.ProcessedAt,
            ResolvedAt = alarm.ResolvedAt,
            Resolution = alarm.Resolution
        };

        // Cache it
        await _cache.SetAsync(
            CacheKeys.Alarms.ById(request.AlarmId),
            dto,
            CacheExpiration.Medium,
            cancellationToken);

        return Result.Success(dto);
    }
}