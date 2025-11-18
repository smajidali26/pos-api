using MediatR;
using POSApi.Application.Common.Helpers;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Features.Reports.Queries.GetDailySalesReport;

namespace POSApi.Application.Features.Reports.Queries.ExportDailySalesReport;

/// <summary>
/// Query to export daily sales report as CSV file
/// </summary>
public record ExportDailySalesReportQuery(DateTime ReportDate) : IQuery<byte[]>;

/// <summary>
/// Handler for exporting daily sales report to CSV format
/// </summary>
public class ExportDailySalesReportQueryHandler : IQueryHandler<ExportDailySalesReportQuery, byte[]>
{
    private readonly IMediator _mediator;

    public ExportDailySalesReportQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportDailySalesReportQuery request, CancellationToken cancellationToken)
    {
        // Reuse existing query to get the report data
        var reportQuery = new GetDailySalesReportQuery(request.ReportDate);
        var report = await _mediator.Send(reportQuery, cancellationToken);

        // Format as CSV
        return CsvFormatter.FormatDailySalesReport(report);
    }
}
