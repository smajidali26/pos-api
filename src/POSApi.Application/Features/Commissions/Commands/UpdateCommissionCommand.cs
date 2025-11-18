using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record UpdateCommissionCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public decimal Rate { get; init; }
    public decimal? MinimumSaleAmount { get; init; }
    public decimal? MaximumCommission { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public class UpdateCommissionCommandHandler : IRequestHandler<UpdateCommissionCommand, Unit>
{
    private readonly ICommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommissionCommandHandler(
        ICommissionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCommissionCommand request, CancellationToken cancellationToken)
    {
        var commission = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (commission == null)
            throw new KeyNotFoundException($"Commission with ID {request.Id} not found");

        commission.UpdateRate(request.Rate, request.MinimumSaleAmount, request.MaximumCommission);
        commission.UpdateEffectiveDates(request.EffectiveFrom, request.EffectiveTo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
