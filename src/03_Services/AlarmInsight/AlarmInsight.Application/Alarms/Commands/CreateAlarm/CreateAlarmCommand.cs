using BahyWay.SharedKernel.Domain.Primitives;
using MediatR;

namespace AlarmInsight.Application.Alarms.Commands.CreateAlarm;

/// <summary>
/// Command to create a new alarm.
/// PATTERN: CQRS Command
/// </summary>
public sealed record CreateAlarmCommand(
    string Source,
    string Description,
    int SeverityValue,
    string LocationName,
    double Latitude,
    double Longitude
) : IRequest<Result<int>>;