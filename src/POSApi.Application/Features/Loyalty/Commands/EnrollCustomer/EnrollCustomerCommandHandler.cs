using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Loyalty.Commands.EnrollCustomer;

public class EnrollCustomerCommandHandler : ICommandHandler<EnrollCustomerCommand, Guid>
{
    private readonly PosDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollCustomerCommandHandler(PosDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(EnrollCustomerCommand request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found");
        }

        // Check if customer is already enrolled
        var existing = await _context.CustomerLoyalties
            .FirstOrDefaultAsync(cl => cl.CustomerId == request.CustomerId, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException($"Customer is already enrolled in the loyalty program");
        }

        var customerLoyalty = new CustomerLoyalty(request.CustomerId);

        // Assign default tier (Bronze - lowest tier)
        var defaultTier = await _context.CustomerTiers
            .OrderBy(t => t.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultTier != null)
        {
            customerLoyalty.PromoteTier(defaultTier.Id);
        }

        _context.CustomerLoyalties.Add(customerLoyalty);
        await _context.SaveChangesAsync(cancellationToken);

        return customerLoyalty.Id;
    }
}
