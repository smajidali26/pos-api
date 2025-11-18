using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record ActivateCommissionCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}

public class ActivateCommissionCommandHandler : IRequestHandler<ActivateCommissionCommand, Unit>
{
    private readonly ICommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCommissionCommandHandler(
        ICommissionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ActivateCommissionCommand request, CancellationToken cancellationToken)
    {
        var commission = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (commission == null)
            throw new KeyNotFoundException($"Commission with ID {request.Id} not found");

        commission.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
