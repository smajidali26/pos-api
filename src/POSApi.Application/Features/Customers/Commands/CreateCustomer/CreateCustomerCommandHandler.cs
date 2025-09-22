using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"Customer with email {request.Email} already exists");
        }

        var customer = new Customer(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.State,
            request.ZipCode,
            request.DateOfBirth
        );

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}