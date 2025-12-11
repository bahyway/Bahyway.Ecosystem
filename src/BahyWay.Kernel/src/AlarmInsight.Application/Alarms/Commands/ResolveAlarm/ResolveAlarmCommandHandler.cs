using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.ResolveAlarm;

public sealed class ResolveAlarmCommandHandler : IRequestHandler<ResolveAlarmCommand, Result>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationLogger<ResolveAlarmCommandHandler> _logger;
    private readonly ICacheService _cache;

    public ResolveAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        IUnitOfWork unitOfWork,
        IApplicationLogger<ResolveAlarmCommandHandler> logger,
        ICacheService cache)
    {
        _alarmRepository = alarmRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result> Handle(ResolveAlarmCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving alarm: {AlarmId}", request.AlarmId);

        // Get alarm
        var alarm = await _alarmRepository.GetByIdAsync(request.AlarmId, cancellationToken);

        if (alarm is null)
        {
            _logger.LogWarning("Alarm not found: {AlarmId}", request.AlarmId);
            return Result.Failure(AlarmErrors.NotFound(request.AlarmId));
        }

        // Resolve alarm (domain logic)
        var result = alarm.Resolve(request.Resolution);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to resolve alarm: {Error}", result.Error.Message);
            return result;
        }

        // Mark as modified
        alarm.MarkAsModified("System");

        // Persist
        _alarmRepository.Update(alarm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alarm resolved successfully: {AlarmId}", request.AlarmId);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Alarms.ById(alarm.Id));
        await _cache.RemoveByPatternAsync(CacheKeys.Alarms.Pattern());

        return Result.Success();
    }
}