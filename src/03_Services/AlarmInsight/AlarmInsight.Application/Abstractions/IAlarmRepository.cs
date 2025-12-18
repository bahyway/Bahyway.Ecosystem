using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlarmInsight.Domain.Aggregates;
// IMPORTANT: Ensure this namespace matches where your 'AlarmStatus' enum is defined.
// It is usually in Domain.ValueObjects or Domain.Enums.
using AlarmInsight.Domain.ValueObjects;

namespace AlarmInsight.Application.Abstractions
{
    public interface IAlarmRepository
    {
        // Standard CRUD
        Task AddAsync(Alarm alarm, CancellationToken cancellationToken = default);
        void Update(Alarm alarm);
        void Delete(Alarm alarm);
        Task<Alarm?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        // *** THIS IS THE MISSING LINE ***
        // This makes the method visible to your DailyResetBackgroundJob
        Task<List<Alarm>> GetAlarmsByStatusAsync(AlarmStatus status, CancellationToken cancellationToken = default);
        
        // Add this line inside the interface IAlarmRepository
        Task<List<Alarm>> GetActiveAlarmsAsync(CancellationToken cancellationToken = default);
    }
}