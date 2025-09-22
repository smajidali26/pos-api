using POSApi.Application.Common.DTOs.Reports;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Reports.Queries.GetDailySalesReport;

public class GetDailySalesReportQuery : IQuery<DailySalesReportDto>
{
    public DateTime ReportDate { get; set; }

    public GetDailySalesReportQuery(DateTime reportDate)
    {
        ReportDate = reportDate.Date; // Ensure we only have the date part
    }
}

public class GetDailySalesReportQueryHandler : IQueryHandler<GetDailySalesReportQuery, DailySalesReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDailySalesReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DailySalesReportDto> Handle(GetDailySalesReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.ReportDate.Date;
        var endDate = startDate.AddDays(1);

        // Get all completed orders for the specified date
        var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

        var report = new DailySalesReportDto
        {
            ReportDate = request.ReportDate,
            TotalOrders = completedOrders.Count,
            TotalSales = completedOrders.Sum(o => o.TotalAmount),
            TotalTax = completedOrders.Sum(o => o.TaxAmount),
            TotalDiscounts = completedOrders.Sum(o => o.DiscountAmount),
            NetSales = completedOrders.Sum(o => o.SubTotal)
        };

        // Payment method breakdown
        report.PaymentMethodBreakdown = completedOrders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentMethodSummary
            {
                PaymentMethod = g.Key,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount),
                Percentage = report.TotalSales > 0 ? (g.Sum(o => o.TotalAmount) / report.TotalSales) * 100 : 0
            })
            .OrderByDescending(p => p.TotalAmount)
            .ToList();

        // Hourly sales breakdown
        report.HourlySalesBreakdown = completedOrders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new HourlySales
            {
                Hour = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(h => h.Hour)
            .ToList();

        // Top selling products
        var productSales = completedOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                SKU = g.Key.SKU,
                QuantitySold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(10)
            .ToList();

        report.TopSellingProducts = productSales;

        // Cashier performance
        report.CashierPerformance = completedOrders
            .GroupBy(o => new { o.CashierId, o.Cashier.FullName })
            .Select(g => new CashierPerformanceDto
            {
                CashierId = g.Key.CashierId,
                CashierName = g.Key.FullName,
                OrdersProcessed = g.Count(),
                TotalSales = g.Sum(o => o.TotalAmount),
                AverageOrderValue = g.Average(o => o.TotalAmount)
            })
            .OrderByDescending(c => c.TotalSales)
            .ToList();

        return report;
    }
}