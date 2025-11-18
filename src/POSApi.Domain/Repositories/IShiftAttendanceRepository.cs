using POSApi.Domain.Entities;

namespace POSApi.Domain.Repositories;

public interface IShiftAttendanceRepository : IRepository<ShiftAttendance>
{
    Task<List<ShiftAttendance>> GetByShiftIdAsync(Guid shiftId, CancellationToken cancellationToken = default);
}
