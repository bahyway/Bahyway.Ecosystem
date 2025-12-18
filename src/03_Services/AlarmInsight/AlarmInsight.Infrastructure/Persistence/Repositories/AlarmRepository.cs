using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // REQUIRED for ToListAsync()
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlarmInsight.Application.Abstractions;
using AlarmInsight.Domain.Aggregates;

namespace AlarmInsight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of IAlarmRepository using EF Core.
/// Handles all database operations for Alarm aggregate.
/// </summary>
//internal sealed class AlarmRepository : IAlarmRepository
public sealed class AlarmRepository : IAlarmRepository
{
    private readonly AlarmInsightDbContext _context;

    public AlarmRepository(AlarmInsightDbContext context)
    {
        _context = context;
    }

    public async Task<Alarm?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Alarms
            .Include(a => a.Notes) // Eager load notes
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Alarm>> GetActiveAlarmsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Alarms
            .Include(a => a.Notes)
            .Where(a => a.Status == AlarmStatus.Pending ||
                       a.Status == AlarmStatus.Processing)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Alarm>> GetByLocationAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        // Search by location Name property (not Address)
        return await _context.Alarms
            .Include(a => a.Notes)
            .Where(a => a.Location.Name.Contains(location))
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Alarm>> GetAlarmsByStatusAsync(
        AlarmStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Alarms
            .Include(a => a.Notes)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Alarm>> GetRecentAlarmsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _context.Alarms
            .Include(a => a.Notes)
            .OrderByDescending(a => a.OccurredAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        await _context.Alarms.AddAsync(alarm, cancellationToken);
    }

    public void Update(Alarm alarm)
    {
        _context.Alarms.Update(alarm);
    }

    public void Delete(Alarm alarm)
    {
        _context.Alarms.Remove(alarm);
    }
    //public async Task<List<Alarm>> GetAlarmsByStatusAsync(string status)
    //{
    //    return await _context.Alarms
    //        .Where(a => a.Status == status) // Adjust 'Status' to match your Entity property name
    //        .ToListAsync();
    //}
    public async Task<List<Alarm>> GetAlarmsByStatusAsync(string status)
    {
        // 1. Try to parse the string into the Enum
        if (Enum.TryParse<AlarmStatus>(status, true, out var statusEnum))
        {
            return await _context.Alarms
                .Where(a => a.Status == statusEnum) // Compare Enum to Enum
                .ToListAsync();
        }

        // 2. If parsing fails, return empty list
        return new List<Alarm>();
    }

}