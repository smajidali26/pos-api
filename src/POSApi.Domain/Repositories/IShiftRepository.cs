using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface IShiftRepository : IRepository<Shift>
{
    Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<Shift>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<List<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);
    Task<List<Shift>> GetUpcomingShiftsAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<Shift?> GetByIdWithAttendanceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Shift>> GetByEmployeeAndDateAsync(Guid employeeProfileId, DateTime date, CancellationToken cancellationToken = default);
}
