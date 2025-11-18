using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Repositories;

public class ShiftAttendanceRepository : Repository<ShiftAttendance>, IShiftAttendanceRepository
{
    public ShiftAttendanceRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<List<ShiftAttendance>> GetByShiftIdAsync(Guid shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ShiftId == shiftId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
