using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNumberGenerator _orderNumberGenerator;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IOrderNumberGenerator orderNumberGenerator)
    {
        _unitOfWork = unitOfWork;
        _orderNumberGenerator = orderNumberGenerator;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Verify cashier exists
        var cashier = await _unitOfWork.Users.GetByIdAsync(request.CashierId, cancellationToken);
        if (cashier == null)
        {
            throw new InvalidOperationException($"Cashier with ID {request.CashierId} not found");
        }

        // Verify customer exists if provided
        if (request.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found");
            }
        }

        // Generate order number
        var orderNumber = _orderNumberGenerator.GenerateOrderNumber();

        // Create order
        var order = new Order(orderNumber, request.CashierId, request.CustomerId);

        // Add order items and validate products
        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
            }

            order.AddOrderItem(item.ProductId, item.Quantity, item.UnitPrice, item.DiscountAmount);
            
            // Reduce product stock
            product.ReduceStock(item.Quantity);
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}