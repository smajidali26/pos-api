using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockTransfers.Commands.CreateStockTransfer;

public class CreateStockTransferCommandHandler : ICommandHandler<CreateStockTransferCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        // Validate from location exists
        var fromLocation = await _unitOfWork.Context.Locations.FindAsync(new object[] { request.FromLocationId }, cancellationToken);
        if (fromLocation == null)
            throw new InvalidOperationException($"From location with ID {request.FromLocationId} not found");

        // Validate to location exists
        var toLocation = await _unitOfWork.Context.Locations.FindAsync(new object[] { request.ToLocationId }, cancellationToken);
        if (toLocation == null)
            throw new InvalidOperationException($"To location with ID {request.ToLocationId} not found");

        // Generate transfer number
        var transferNumber = await GenerateTransferNumberAsync(cancellationToken);

        // Create stock transfer
        var transfer = new StockTransfer(
            transferNumber,
            request.ProductId,
            request.FromLocationId,
            request.ToLocationId,
            request.RequestedQuantity,
            request.UserId,
            request.Notes ?? string.Empty
        );

        await _unitOfWork.Context.StockTransfers.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }

    private async Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = "ST";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _unitOfWork.Context.StockTransfers.CountAsync(cancellationToken);
        var sequence = (count + 1).ToString("D5");

        return $"{prefix}-{date}-{sequence}";
    }
}
