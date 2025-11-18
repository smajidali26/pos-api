using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Queries.GetPointsExpiring;

public class GetPointsExpiringQuery : IQuery<List<LoyaltyTransactionDto>>
{
    public Guid CustomerId { get; set; }
    public int DaysThreshold { get; set; } = 30;

    public GetPointsExpiringQuery(Guid customerId, int daysThreshold = 30)
    {
        CustomerId = customerId;
        DaysThreshold = daysThreshold;
    }
}
