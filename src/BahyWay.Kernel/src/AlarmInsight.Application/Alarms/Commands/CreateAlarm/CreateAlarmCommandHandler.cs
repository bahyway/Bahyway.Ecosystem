using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain.Aggregates;
using AlarmInsight.Domain.ValueObjects;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.CreateAlarm;

/// <summary>
/// Handler for CreateAlarmCommand.
/// DEMONSTRATES: How to use ALL SharedKernel abstractions together!
/// </summary>
public sealed class CreateAlarmCommandHandler : IRequestHandler<CreateAlarmCommand, Result<int>>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationLogger<CreateAlarmCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly IBackgroundJobService _backgroundJobs;

    public CreateAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        IUnitOfWork unitOfWork,
        IApplicationLogger<CreateAlarmCommandHandler> logger,
        ICacheService cache,
        IBackgroundJobService backgroundJobs)
    {
        _alarmRepository = alarmRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<Result<int>> Handle(
        CreateAlarmCommand request,
        CancellationToken cancellationToken)
    {
        // 1. LOG
        _logger.LogInformation(
            "Creating alarm from source: {Source}, Severity: {Severity}",
            request.Source,
            request.SeverityValue);

        // 2. CREATE VALUE OBJECTS
        var locationResult = Location.Create(
            request.LocationName,
            request.Latitude,
            request.Longitude);

        if (locationResult.IsFailure)
        {
            _logger.LogWarning("Invalid location: {Error}", locationResult.Error.Message);
            return Result.Failure<int>(locationResult.Error);
        }

        var severityResult = AlarmSeverity.FromValue(request.SeverityValue);

        if (severityResult.IsFailure)
        {
            _logger.LogWarning("Invalid severity: {Error}", severityResult.Error.Message);
            return Result.Failure<int>(severityResult.Error);
        }

        // 3. CREATE DOMAIN ENTITY
        var alarmResult = Alarm.Create(
            request.Source,
            request.Description,
            severityResult.Value,
            locationResult.Value);

        if (alarmResult.IsFailure)
        {
            _logger.LogWarning("Failed to create alarm: {Error}", alarmResult.Error.Message);
            return Result.Failure<int>(alarmResult.Error);
        }

        var alarm = alarmResult.Value;

        // 4. SET AUDIT INFO
        alarm.MarkAsCreated("System");

        // 5. PERSIST
        await _alarmRepository.AddAsync(alarm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alarm created successfully with ID: {AlarmId}", alarm.Id);

        // 6. INVALIDATE CACHE
        await _cache.RemoveByPatternAsync(CacheKeys.Alarms.Pattern());

        // 7. ENQUEUE BACKGROUND JOB
        // Note: Actual job implementation will be in Infrastructure
        _logger.LogDebug("Alarm processing initiated for: {AlarmId}", alarm.Id);

        // 8. RETURN SUCCESS
        return Result.Success(alarm.Id);
    }
}