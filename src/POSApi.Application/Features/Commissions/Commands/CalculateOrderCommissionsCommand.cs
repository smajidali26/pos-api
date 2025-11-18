using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record CalculateOrderCommissionsCommand : IRequest<List<Guid>>
{
    public Guid OrderId { get; init; }
    public Guid EmployeeProfileId { get; init; }
    public string? EmployeeRole { get; init; }
}

public class CalculateOrderCommissionsCommandHandler : IRequestHandler<CalculateOrderCommissionsCommand, List<Guid>>
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly ICommissionTransactionRepository _transactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CalculateOrderCommissionsCommandHandler(
        ICommissionRepository commissionRepository,
        ICommissionTransactionRepository transactionRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _commissionRepository = commissionRepository;
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Guid>> Handle(CalculateOrderCommissionsCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found");

        // Get applicable commissions for this employee
        var commissions = await _commissionRepository.GetApplicableCommissionsAsync(
            request.EmployeeProfileId, 
            request.EmployeeRole, 
            cancellationToken);

        var transactionIds = new List<Guid>();

        foreach (var commission in commissions)
        {
            // Check if commission applies to this order
            if (!commission.IsApplicableToOrder(order, request.EmployeeProfileId, request.EmployeeRole))
                continue;

            // Calculate commission based on basis type
            decimal commissionAmount = 0;
            if (commission.CommissionBasis == CommissionBasis.TotalSale)
            {
                commissionAmount = commission.CalculateCommission(order.TotalAmount);
                
                if (commissionAmount > 0)
                {
                    var transaction = new CommissionTransaction(
                        request.EmployeeProfileId,
                        commission.Id,
                        order.Id,
                        DateTime.UtcNow,
                        order.TotalAmount,
                        commissionAmount
                    );

                    await _transactionRepository.AddAsync(transaction, cancellationToken);
                    transactionIds.Add(transaction.Id);
                }
            }
            else if (commission.CommissionBasis == CommissionBasis.ItemsSold)
            {
                // Calculate commission per item
                foreach (var item in order.OrderItems)
                {
                    bool itemApplicable = commission.IsApplicableToProduct(
                        item.ProductId, 
                        item.Product?.CategoryId);

                    if (itemApplicable)
                    {
                        var itemCommission = commission.CalculateCommission(item.Price * item.Quantity, item.Quantity);
                        
                        if (itemCommission > 0)
                        {
                            var transaction = new CommissionTransaction(
                                request.EmployeeProfileId,
                                commission.Id,
                                order.Id,
                                DateTime.UtcNow,
                                item.Price * item.Quantity,
                                itemCommission,
                                item.Id
                            );

                            await _transactionRepository.AddAsync(transaction, cancellationToken);
                            transactionIds.Add(transaction.Id);
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return transactionIds;
    }
}
