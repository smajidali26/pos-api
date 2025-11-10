using MediatR;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Sizes.Commands.DeleteSize;

public class DeleteSizeCommandHandler : ICommandHandler<DeleteSizeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSizeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSizeCommand request, CancellationToken cancellationToken)
    {
        var size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);
        if (size == null)
        {
            throw new InvalidOperationException($"Size with ID {request.Id} not found");
        }

        // Check if any products are using this size
        var productsWithSize = await _unitOfWork.Products.GetBySizeIdAsync(request.Id, cancellationToken);
        if (productsWithSize.Any())
        {
            throw new InvalidOperationException($"Cannot delete size '{size.Name}' because it is being used by {productsWithSize.Count()} product(s)");
        }

        await _unitOfWork.Sizes.DeleteAsync(size, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
