using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Queries.GetActiveAlarms;

public sealed record GetActiveAlarmsQuery() : IRequest<Result<IEnumerable<AlarmSummaryDto>>>;