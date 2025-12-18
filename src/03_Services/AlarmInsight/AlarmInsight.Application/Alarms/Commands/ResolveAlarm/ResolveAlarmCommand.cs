using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.ResolveAlarm;

public sealed record ResolveAlarmCommand(
    int AlarmId,
    string Resolution
) : IRequest<Result>;