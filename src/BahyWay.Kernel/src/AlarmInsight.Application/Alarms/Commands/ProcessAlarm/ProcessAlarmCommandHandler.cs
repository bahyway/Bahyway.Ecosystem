using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.ProcessAlarm;

public sealed class ProcessAlarmCommandHandler : IRequestHandler<ProcessAlarmCommand, Result>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationLogger<ProcessAlarmCommandHandler> _logger;
    private readonly ICacheService _cache;

    public ProcessAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        IUnitOfWork unitOfWork,
        IApplicationLogger<ProcessAlarmCommandHandler> logger,
        ICacheService cache)
    {
        _alarmRepository = alarmRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result> Handle(ProcessAlarmCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing alarm: {AlarmId}", request.AlarmId);

        // Get alarm
        var alarm = await _alarmRepository.GetByIdAsync(request.AlarmId, cancellationToken);

        if (alarm is null)
        {
            _logger.LogWarning("Alarm not found: {AlarmId}", request.AlarmId);
            return Result.Failure(AlarmErrors.NotFound(request.AlarmId));
        }

        // Process alarm (domain logic)
        var result = alarm.Process();

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to process alarm: {Error}", result.Error.Message);
            return result;
        }

        // Mark as modified
        alarm.MarkAsModified("System");

        // Persist
        _alarmRepository.Update(alarm);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alarm processed successfully: {AlarmId}", request.AlarmId);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Alarms.ById(alarm.Id));

        return Result.Success();
    }
}