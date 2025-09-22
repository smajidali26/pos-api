using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class VendorRepository : Repository<Vendor>, IVendorRepository
{
    public VendorRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Vendor?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.Name == name, cancellationToken);
    }

    public async Task<Vendor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.Email == email, cancellationToken);
    }

    public async Task<Vendor?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(v => v.TaxId == taxId, cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetByTypeAsync(VendorType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetActiveVendorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.IsActive && v.Status == VendorStatus.Active)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Name.Contains(searchTerm) || 
                       v.CompanyName.Contains(searchTerm) || 
                       v.Email.Contains(searchTerm) ||
                       v.ContactPerson.Contains(searchTerm))
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vendor>> GetVendorsWithCreditLimitExceededAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.CreditLimit > 0 && v.CurrentBalance > v.CreditLimit)
            .OrderByDescending(v => v.CurrentBalance - v.CreditLimit)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.Products)
            .Include(v => v.PurchaseOrders)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Vendor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.Products)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
    }
}