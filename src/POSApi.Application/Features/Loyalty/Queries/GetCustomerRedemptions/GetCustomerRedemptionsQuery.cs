using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Queries.GetCustomerRedemptions;

public class GetCustomerRedemptionsQuery : IQuery<List<RewardRedemptionDto>>
{
    public Guid CustomerId { get; set; }
    public bool? IncludeUsed { get; set; } = true;

    public GetCustomerRedemptionsQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}
