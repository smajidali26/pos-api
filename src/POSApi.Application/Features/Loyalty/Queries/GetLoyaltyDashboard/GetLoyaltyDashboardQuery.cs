using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;

public class GetLoyaltyDashboardQuery : IQuery<LoyaltyDashboardDto>
{
    public int TopCustomersCount { get; set; } = 10;
    public int MonthsBack { get; set; } = 6;
}
