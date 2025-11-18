using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record DeactivateCommissionCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}

public class DeactivateCommissionCommandHandler : IRequestHandler<DeactivateCommissionCommand, Unit>
{
    private readonly ICommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCommissionCommandHandler(
        ICommissionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeactivateCommissionCommand request, CancellationToken cancellationToken)
    {
        var commission = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (commission == null)
            throw new KeyNotFoundException($"Commission with ID {request.Id} not found");

        commission.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
