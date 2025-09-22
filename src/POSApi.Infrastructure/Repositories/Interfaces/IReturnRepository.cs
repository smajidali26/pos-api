using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IReturnRepository : IRepository<Return>
{
    Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Return>> GetByOriginalOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Return>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Return>> GetByProcessedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundsForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}