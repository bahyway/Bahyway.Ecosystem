using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Queries.GetAlarm;

public sealed record GetAlarmQuery(int AlarmId) : IRequest<Result<AlarmDto>>;