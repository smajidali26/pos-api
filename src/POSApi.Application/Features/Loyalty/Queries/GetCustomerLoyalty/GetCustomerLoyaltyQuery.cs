using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Queries.GetCustomerLoyalty;

public class GetCustomerLoyaltyQuery : IQuery<CustomerLoyaltyDto?>
{
    public Guid CustomerId { get; set; }

    public GetCustomerLoyaltyQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}
