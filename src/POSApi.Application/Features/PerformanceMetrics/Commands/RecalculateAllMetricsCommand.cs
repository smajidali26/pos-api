using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Commands;

public record RecalculateAllMetricsCommand : IRequest<int>
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public MetricPeriodType PeriodType { get; init; }
}

public class RecalculateAllMetricsCommandHandler : IRequestHandler<RecalculateAllMetricsCommand, int>
{
    private readonly IEmployeeProfileRepository _employeeRepository;
    private readonly IMediator _mediator;

    public RecalculateAllMetricsCommandHandler(
        IEmployeeProfileRepository employeeRepository,
        IMediator mediator)
    {
        _employeeRepository = employeeRepository;
        _mediator = mediator;
    }

    public async Task<int> Handle(RecalculateAllMetricsCommand request, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var activeEmployees = employees.Where(e => e.Status == EmploymentStatus.Active).ToList();

        int calculatedCount = 0;

        foreach (var employee in activeEmployees)
        {
            try
            {
                await _mediator.Send(new CalculatePerformanceMetricsCommand
                {
                    EmployeeProfileId = employee.Id,
                    PeriodStart = request.PeriodStart,
                    PeriodEnd = request.PeriodEnd,
                    PeriodType = request.PeriodType
                }, cancellationToken);

                calculatedCount++;
            }
            catch
            {
                // Skip employees with calculation errors
                continue;
            }
        }

        return calculatedCount;
    }
}
