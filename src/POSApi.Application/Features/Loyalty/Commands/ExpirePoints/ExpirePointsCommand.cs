using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Loyalty.Commands.ExpirePoints;

public class ExpirePointsCommand : ICommand<int>
{
    // This command will be executed by a background job to expire points for all customers
}
