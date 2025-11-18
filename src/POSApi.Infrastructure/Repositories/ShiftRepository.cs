using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.EmployeeProfile).ThenInclude(e => e.User)
            .Include(s => s.Store)
            .Where(s => s.EmployeeProfileId == employeeProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Shift>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.EmployeeProfile).ThenInclude(e => e.User)
            .Include(s => s.Store)
            .Where(s => s.ScheduledStartTime >= start && s.ScheduledStartTime < end)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.EmployeeProfile).ThenInclude(e => e.User)
            .Include(s => s.Store)
            .Where(s => s.Status == ShiftStatus.InProgress)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Shift>> GetUpcomingShiftsAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.EmployeeProfile).ThenInclude(e => e.User)
            .Include(s => s.Store)
            .Where(s => s.EmployeeProfileId == employeeProfileId &&
                       s.Status == ShiftStatus.Scheduled &&
                       s.ScheduledStartTime > now)
            .OrderBy(s => s.ScheduledStartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shift?> GetByIdWithAttendanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.EmployeeProfile).ThenInclude(e => e.User)
            .Include(s => s.Store)
            .Include(s => s.Attendances)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Shift>> GetByEmployeeAndDateAsync(Guid employeeProfileId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _dbSet
            .Where(s => s.EmployeeProfileId == employeeProfileId &&
                       s.ScheduledStartTime >= startOfDay &&
                       s.ScheduledStartTime < endOfDay)
            .ToListAsync(cancellationToken);
    }
}
