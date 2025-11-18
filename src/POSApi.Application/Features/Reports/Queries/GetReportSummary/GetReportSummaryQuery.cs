using MediatR;
using POSApi.Application.Common.DTOs.Reports;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Features.Reports.Queries.GetDailySalesReport;
using POSApi.Application.Features.Reports.Queries.GetInventoryReport;

namespace POSApi.Application.Features.Reports.Queries.GetReportSummary;

/// <summary>
/// Query to get a comprehensive daily report summary combining sales and inventory data
/// </summary>
public record GetReportSummaryQuery(DateTime ReportDate) : IQuery<ReportSummaryDto>;

/// <summary>
/// Handler for generating comprehensive report summary
/// </summary>
public class GetReportSummaryQueryHandler : IQueryHandler<GetReportSummaryQuery, ReportSummaryDto>
{
    private readonly IMediator _mediator;

    public GetReportSummaryQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ReportSummaryDto> Handle(GetReportSummaryQuery request, CancellationToken cancellationToken)
    {
        // Fetch both reports in parallel for better performance
        var salesReportTask = _mediator.Send(new GetDailySalesReportQuery(request.ReportDate), cancellationToken);
        var inventoryReportTask = _mediator.Send(new GetInventoryReportQuery(request.ReportDate), cancellationToken);

        await Task.WhenAll(salesReportTask, inventoryReportTask);

        var salesReport = await salesReportTask;
        var inventoryReport = await inventoryReportTask;

        var summary = FormatReportSummary(request.ReportDate, salesReport, inventoryReport);

        return new ReportSummaryDto
        {
            ReportDate = request.ReportDate,
            Summary = summary
        };
    }

    private static string FormatReportSummary(DateTime reportDate, DailySalesReportDto salesReport, InventoryReportDto inventoryReport)
    {
        var avgOrderValue = salesReport.TotalOrders > 0 ? salesReport.TotalSales / salesReport.TotalOrders : 0;

        var topProducts = string.Join("\n",
            salesReport.TopSellingProducts
                .Take(5)
                .Select(p => $"  • {p.ProductName}: {p.QuantitySold} units, {p.TotalRevenue:C}"));

        var cashierPerformance = string.Join("\n",
            salesReport.CashierPerformance
                .Select(c => $"  • {c.CashierName}: {c.OrdersProcessed} orders, {c.TotalSales:C}"));

        return $"""
            Daily Report Summary - {reportDate:yyyy-MM-dd}
            ===============================================

            SALES PERFORMANCE:
            • Total Orders: {salesReport.TotalOrders}
            • Total Sales: {salesReport.TotalSales:C}
            • Net Sales: {salesReport.NetSales:C}
            • Average Order Value: {avgOrderValue:C}

            TOP PERFORMING PRODUCTS:
            {topProducts}

            INVENTORY STATUS:
            • Total Products: {inventoryReport.TotalProducts}
            • Low Stock Items: {inventoryReport.LowStockProducts}
            • Out of Stock Items: {inventoryReport.OutOfStockProducts}
            • Total Inventory Value: {inventoryReport.TotalInventoryValue:C}

            CASHIER PERFORMANCE:
            {cashierPerformance}
            """;
    }
}
