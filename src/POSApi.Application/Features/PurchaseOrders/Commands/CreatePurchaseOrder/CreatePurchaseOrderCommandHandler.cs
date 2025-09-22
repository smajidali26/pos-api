using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler : ICommandHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public CreatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IOrderNumberGenerator orderNumberGenerator)
    {
        _unitOfWork = unitOfWork;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // Verify vendor exists and is active
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException($"Vendor with ID {request.VendorId} not found");
        }

        if (!vendor.CanOrder())
        {
            throw new InvalidOperationException($"Cannot create purchase order for vendor {vendor.Name}. Vendor is not active or has exceeded credit limit.");
        }

        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.CreatedByUserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.CreatedByUserId} not found");
        }

        // Generate purchase order number (reuse the existing generator for now)
        var orderNumber = $"PO-{_orderNumberGenerator.GenerateOrderNumber()}";

        // Create purchase order
        var purchaseOrder = new PurchaseOrder(
            orderNumber, 
            request.VendorId, 
            request.CreatedByUserId, 
            request.ExpectedDeliveryDate);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            purchaseOrder.UpdateNotes(request.Notes);
        }

        // Add items and validate products
        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException($"Cannot order inactive product {product.Name}");
            }

            purchaseOrder.AddItem(item.ProductId, item.Quantity, item.UnitCost);
        }

        await _unitOfWork.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return purchaseOrder.Id;
    }
}