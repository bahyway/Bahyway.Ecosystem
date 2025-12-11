using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.ProcessAlarm;

public sealed record ProcessAlarmCommand(int AlarmId) : IRequest<Result>;