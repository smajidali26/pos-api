using MediatR;
using POSApi.Application.Common.Helpers;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Features.Reports.Queries.GetInventoryReport;

namespace POSApi.Application.Features.Reports.Queries.ExportInventoryReport;

/// <summary>
/// Query to export inventory report as CSV file
/// </summary>
public record ExportInventoryReportQuery(DateTime? AsOfDate = null) : IQuery<byte[]>;

/// <summary>
/// Handler for exporting inventory report to CSV format
/// </summary>
public class ExportInventoryReportQueryHandler : IQueryHandler<ExportInventoryReportQuery, byte[]>
{
    private readonly IMediator _mediator;

    public ExportInventoryReportQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportInventoryReportQuery request, CancellationToken cancellationToken)
    {
        // Reuse existing query to get the report data
        var reportQuery = new GetInventoryReportQuery(request.AsOfDate);
        var report = await _mediator.Send(reportQuery, cancellationToken);

        // Format as CSV
        return CsvFormatter.FormatInventoryReport(report);
    }
}
