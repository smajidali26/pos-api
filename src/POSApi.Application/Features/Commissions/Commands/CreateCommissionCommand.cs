using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record CreateCommissionCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CommissionType CommissionType { get; init; }
    public CommissionBasis CommissionBasis { get; init; }
    public decimal Rate { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public decimal? MinimumSaleAmount { get; init; }
    public decimal? MaximumCommission { get; init; }
    public Guid? EmployeeProfileId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Role { get; init; }
}

public class CreateCommissionCommandHandler : IRequestHandler<CreateCommissionCommand, Guid>
{
    private readonly ICommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommissionCommandHandler(
        ICommissionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCommissionCommand request, CancellationToken cancellationToken)
    {
        var commission = new Commission(
            request.Name,
            request.Description,
            request.CommissionType,
            request.CommissionBasis,
            request.Rate,
            request.EffectiveFrom,
            request.MinimumSaleAmount,
            request.MaximumCommission,
            request.EmployeeProfileId,
            request.ProductId,
            request.CategoryId,
            request.Role
        );

        await _repository.AddAsync(commission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return commission.Id;
    }
}
