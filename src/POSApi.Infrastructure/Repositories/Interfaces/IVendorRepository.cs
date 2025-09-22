using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IVendorRepository : IRepository<Vendor>
{
    Task<Vendor?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vendor>> GetByTypeAsync(VendorType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vendor>> GetVendorsWithCreditLimitExceededAsync(CancellationToken cancellationToken = default);
}

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetOverduePurchaseOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetPendingReceiptAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PurchaseOrder>> GetByCreatedUserAsync(Guid userId, CancellationToken cancellationToken = default);
}