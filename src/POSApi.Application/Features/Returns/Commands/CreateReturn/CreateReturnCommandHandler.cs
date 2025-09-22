using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.Returns.Commands.CreateReturn;

public class CreateReturnCommandHandler : ICommandHandler<CreateReturnCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public CreateReturnCommandHandler(IUnitOfWork unitOfWork, IOrderNumberGenerator orderNumberGenerator)
    {
        _unitOfWork = unitOfWork;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<Guid> Handle(CreateReturnCommand request, CancellationToken cancellationToken)
    {
        // Verify original order exists
        var originalOrder = await _unitOfWork.Orders.GetByIdAsync(request.OriginalOrderId, cancellationToken);
        if (originalOrder == null)
        {
            throw new InvalidOperationException($"Original order with ID {request.OriginalOrderId} not found");
        }

        // Verify order is completed (can only return completed orders)
        if (originalOrder.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot process return for order in {originalOrder.Status} status");
        }

        // Verify processed by user exists
        var processedByUser = await _unitOfWork.Users.GetByIdAsync(request.ProcessedByUserId, cancellationToken);
        if (processedByUser == null)
        {
            throw new InvalidOperationException($"User with ID {request.ProcessedByUserId} not found");
        }

        // Generate return number
        var returnNumber = $"RET-{_orderNumberGenerator.GenerateOrderNumber()}";

        // Create return
        var return_ = new Return(returnNumber, request.OriginalOrderId, request.ProcessedByUserId, request.Reason, request.Notes);

        // Add return items and validate against original order
        foreach (var itemRequest in request.Items)
        {
            var originalOrderItem = originalOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == itemRequest.ProductId);
            if (originalOrderItem == null)
            {
                throw new InvalidOperationException($"Product {itemRequest.ProductId} was not in the original order");
            }

            // Check if trying to return more than was originally ordered
            var existingReturns = await _unitOfWork.Returns.GetByOriginalOrderIdAsync(request.OriginalOrderId, cancellationToken);
            var alreadyReturnedQuantity = 0;
            
            foreach (var existingReturn in existingReturns)
            {
                if (existingReturn.Status != ReturnStatus.Cancelled)
                {
                    var returnItems = existingReturn.ReturnItems.Where(ri => ri.ProductId == itemRequest.ProductId);
                    alreadyReturnedQuantity += returnItems.Sum(ri => ri.Quantity);
                }
            }

            if (alreadyReturnedQuantity + itemRequest.Quantity > originalOrderItem.Quantity)
            {
                throw new InvalidOperationException($"Cannot return more than originally ordered for product {itemRequest.ProductId}");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(itemRequest.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {itemRequest.ProductId} not found");
            }

            return_.AddReturnItem(itemRequest.ProductId, itemRequest.Quantity, itemRequest.UnitPrice, itemRequest.Reason);
        }

        await _unitOfWork.Returns.AddAsync(return_, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return return_.Id;
    }
}