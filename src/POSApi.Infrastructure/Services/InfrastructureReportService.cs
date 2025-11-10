using System.Text;
using Microsoft.Extensions.Logging;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.DTOs.Reports;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services.Interfaces;

namespace POSApi.Infrastructure.Services;

public class InfrastructureReportService : IInfrastructureReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InfrastructureReportService> _logger;

    public InfrastructureReportService(IUnitOfWork unitOfWork, ILogger<InfrastructureReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DailySalesReportDto> GenerateDailySalesReportAsync(DateTime reportDate, CancellationToken cancellationToken = default)
    {
        var startDate = reportDate.Date;
        var endDate = startDate.AddDays(1);

        // Get all completed orders for the specified date
        var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

        var report = new DailySalesReportDto
        {
            ReportDate = reportDate,
            TotalOrders = completedOrders.Count,
            TotalSales = completedOrders.Sum(o => o.TotalAmount),
            TotalTax = completedOrders.Sum(o => o.TaxAmount),
            TotalDiscounts = completedOrders.Sum(o => o.DiscountAmount),
            NetSales = completedOrders.Sum(o => o.SubtotalAmount)
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

        _logger.LogInformation("Generated daily sales report for {ReportDate} with {TotalOrders} orders", reportDate, report.TotalOrders);
        return report;
    }

    public async Task<InventoryReportDto> GenerateInventoryReportAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var reportDate = asOfDate ?? DateTime.Now.Date;
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var productList = products.ToList();

        var report = new InventoryReportDto
        {
            ReportDate = reportDate,
            TotalProducts = productList.Count,
            LowStockProducts = productList.Count(p => p.StockQuantity <= p.MinStockLevel && p.StockQuantity > 0),
            OutOfStockProducts = productList.Count(p => p.StockQuantity == 0),
            TotalInventoryValue = productList.Sum(p => p.StockQuantity * p.Cost)
        };

        // Product inventory details
        report.ProductInventory = productList
            .Select(p => new ProductInventoryDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                CurrentStock = p.StockQuantity,
                MinStockLevel = p.MinStockLevel,
                UnitCost = p.Cost,
                UnitPrice = p.Price,
                TotalValue = p.StockQuantity * p.Cost,
                StockStatus = GetStockStatus(p.StockQuantity, p.MinStockLevel)
            })
            .OrderBy(p => p.CategoryName)
            .ThenBy(p => p.ProductName)
            .ToList();

        // Category breakdown
        report.CategoryBreakdown = productList
            .GroupBy(p => new { 
                CategoryId = p.CategoryId, 
                CategoryName = p.Category?.Name ?? "Uncategorized" 
            })
            .Select(g => new CategoryInventoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Count(),
                TotalStock = g.Sum(p => p.StockQuantity),
                TotalValue = g.Sum(p => p.StockQuantity * p.Cost)
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();

        _logger.LogInformation("Generated inventory report for {ReportDate} with {TotalProducts} products", reportDate, report.TotalProducts);
        return report;
    }

    public async Task<CustomerAnalyticsReportDto> GenerateCustomerAnalyticsReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        var customerList = customers.ToList();

        var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(startDate, endDate.AddDays(1), cancellationToken);
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

        var newCustomers = customerList.Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate.AddDays(1)).ToList();
        var customersWithOrders = completedOrders.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId!.Value).Distinct().Count();

        var report = new CustomerAnalyticsReportDto
        {
            ReportDate = DateTime.Now.Date,
            TotalCustomers = customerList.Count,
            NewCustomersThisPeriod = newCustomers.Count,
            ActiveCustomers = customersWithOrders,
            AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0,
            CustomerLifetimeValue = CalculateCustomerLifetimeValue(customerList, completedOrders)
        };

        // Top customers by total spent
        var customerOrders = completedOrders
            .Where(o => o.CustomerId.HasValue && o.Customer != null)
            .GroupBy(o => new { o.CustomerId, o.Customer })
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key.CustomerId!.Value,
                CustomerName = g.Key.Customer!.FullName,
                Email = g.Key.Customer.Email,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                LoyaltyPoints = g.Key.Customer.LoyaltyPoints,
                LastPurchase = g.Max(o => o.OrderDate)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(20)
            .ToList();

        report.TopCustomers = customerOrders;

        // Customer segments based on spending
        report.CustomerSegments = CreateCustomerSegments(customerOrders);

        // Loyalty program summary
        report.LoyaltyProgram = new LoyaltyProgramSummary
        {
            TotalLoyaltyMembers = customerList.Count(c => c.LoyaltyPoints > 0),
            TotalPointsIssued = CalculateTotalPointsIssued(completedOrders),
            TotalPointsRedeemed = 0, // Would need point history tracking
            PointsOutstanding = customerList.Sum(c => c.LoyaltyPoints),
            AveragePointsPerCustomer = customerList.Any() ? customerList.Average(c => c.LoyaltyPoints) : 0
        };

        _logger.LogInformation("Generated customer analytics report for {StartDate} to {EndDate}", startDate, endDate);
        return report;
    }

    public async Task<byte[]> ExportDailySalesReportToCsvAsync(DateTime reportDate, CancellationToken cancellationToken = default)
    {
        var report = await GenerateDailySalesReportAsync(reportDate, cancellationToken);
        return GenerateDailySalesCsv(report);
    }

    public async Task<byte[]> ExportInventoryReportToCsvAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var report = await GenerateInventoryReportAsync(asOfDate, cancellationToken);
        return GenerateInventoryCsv(report);
    }

    public async Task<string> GenerateReportSummaryAsync(DateTime reportDate, CancellationToken cancellationToken = default)
    {
        var salesReport = await GenerateDailySalesReportAsync(reportDate, cancellationToken);
        var inventoryReport = await GenerateInventoryReportAsync(reportDate, cancellationToken);

        var summary = $"""
            Daily Report Summary - {reportDate:yyyy-MM-dd}
            ===============================================
            
            SALES PERFORMANCE:
            � Total Orders: {salesReport.TotalOrders}
            � Total Sales: {salesReport.TotalSales:C}
            � Net Sales: {salesReport.NetSales:C}
            � Average Order Value: {(salesReport.TotalOrders > 0 ? salesReport.TotalSales / salesReport.TotalOrders : 0):C}
            
            TOP PERFORMING PRODUCTS:
            {string.Join("\n", salesReport.TopSellingProducts.Take(5).Select(p => $"� {p.ProductName}: {p.QuantitySold} units, {p.TotalRevenue:C}"))}
            
            INVENTORY STATUS:
            � Total Products: {inventoryReport.TotalProducts}
            � Low Stock Items: {inventoryReport.LowStockProducts}
            � Out of Stock Items: {inventoryReport.OutOfStockProducts}
            � Total Inventory Value: {inventoryReport.TotalInventoryValue:C}
            
            CASHIER PERFORMANCE:
            {string.Join("\n", salesReport.CashierPerformance.Select(c => $"� {c.CashierName}: {c.OrdersProcessed} orders, {c.TotalSales:C}"))}
            """;

        return summary;
    }

    private static string GetStockStatus(int currentStock, int minStockLevel)
    {
        if (currentStock == 0)
            return "Out of Stock";
        if (currentStock <= minStockLevel)
            return "Low Stock";
        return "Normal";
    }

    private static decimal CalculateCustomerLifetimeValue(List<Customer> customers, List<Order> orders)
    {
        if (!customers.Any()) return 0;

        var customerOrderTotals = orders
            .Where(o => o.CustomerId.HasValue)
            .GroupBy(o => o.CustomerId)
            .Select(g => g.Sum(o => o.TotalAmount))
            .ToList();

        return customerOrderTotals.Any() ? customerOrderTotals.Average() : 0;
    }

    private static List<CustomerSegmentDto> CreateCustomerSegments(List<TopCustomerDto> customers)
    {
        if (!customers.Any()) return new List<CustomerSegmentDto>();

        var segments = new List<CustomerSegmentDto>();

        // VIP customers (top 10% by spending)
        var vipThreshold = customers.OrderByDescending(c => c.TotalSpent).Take((int)Math.Ceiling(customers.Count * 0.1)).LastOrDefault()?.TotalSpent ?? 0;
        var vipCustomers = customers.Where(c => c.TotalSpent >= vipThreshold).ToList();

        // Regular customers (next 40%)
        var regularThreshold = customers.OrderByDescending(c => c.TotalSpent).Take((int)Math.Ceiling(customers.Count * 0.5)).LastOrDefault()?.TotalSpent ?? 0;
        var regularCustomers = customers.Where(c => c.TotalSpent < vipThreshold && c.TotalSpent >= regularThreshold).ToList();

        // Casual customers (remaining 50%)
        var casualCustomers = customers.Where(c => c.TotalSpent < regularThreshold).ToList();

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "VIP Customers",
            CustomerCount = vipCustomers.Count,
            TotalRevenue = vipCustomers.Sum(c => c.TotalSpent),
            AverageOrderValue = vipCustomers.Any() ? vipCustomers.Average(c => c.TotalSpent / c.TotalOrders) : 0
        });

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "Regular Customers",
            CustomerCount = regularCustomers.Count,
            TotalRevenue = regularCustomers.Sum(c => c.TotalSpent),
            AverageOrderValue = regularCustomers.Any() ? regularCustomers.Average(c => c.TotalSpent / c.TotalOrders) : 0
        });

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "Casual Customers",
            CustomerCount = casualCustomers.Count,
            TotalRevenue = casualCustomers.Sum(c => c.TotalSpent),
            AverageOrderValue = casualCustomers.Any() ? casualCustomers.Average(c => c.TotalSpent / c.TotalOrders) : 0
        });

        return segments;
    }

    private static decimal CalculateTotalPointsIssued(List<Order> orders)
    {
        // Assuming 1 point per dollar spent for customers
        return orders.Where(o => o.CustomerId.HasValue).Sum(o => Math.Floor(o.TotalAmount));
    }

    private static byte[] GenerateDailySalesCsv(DailySalesReportDto report)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine($"Daily Sales Report - {report.ReportDate:yyyy-MM-dd}");
        csv.AppendLine();
        
        // Summary
        csv.AppendLine("Summary");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Orders,{report.TotalOrders}");
        csv.AppendLine($"Total Sales,{report.TotalSales}");
        csv.AppendLine($"Net Sales,{report.NetSales}");
        csv.AppendLine($"Total Tax,{report.TotalTax}");
        csv.AppendLine($"Total Discounts,{report.TotalDiscounts}");
        csv.AppendLine();
        
        // Top Products
        csv.AppendLine("Top Selling Products");
        csv.AppendLine("Product Name,SKU,Quantity Sold,Total Revenue");
        foreach (var product in report.TopSellingProducts)
        {
            csv.AppendLine($"{product.ProductName},{product.SKU},{product.QuantitySold},{product.TotalRevenue}");
        }
        csv.AppendLine();
        
        // Payment Methods
        csv.AppendLine("Payment Method Breakdown");
        csv.AppendLine("Payment Method,Order Count,Total Amount,Percentage");
        foreach (var payment in report.PaymentMethodBreakdown)
        {
            csv.AppendLine($"{payment.PaymentMethod},{payment.OrderCount},{payment.TotalAmount},{payment.Percentage:F2}%");
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] GenerateInventoryCsv(InventoryReportDto report)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine($"Inventory Report - {report.ReportDate:yyyy-MM-dd}");
        csv.AppendLine();
        
        // Summary
        csv.AppendLine("Summary");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Products,{report.TotalProducts}");
        csv.AppendLine($"Low Stock Products,{report.LowStockProducts}");
        csv.AppendLine($"Out of Stock Products,{report.OutOfStockProducts}");
        csv.AppendLine($"Total Inventory Value,{report.TotalInventoryValue}");
        csv.AppendLine();
        
        // Product Details
        csv.AppendLine("Product Inventory");
        csv.AppendLine("Product Name,SKU,Category,Current Stock,Min Stock Level,Unit Cost,Unit Price,Total Value,Status");
        foreach (var product in report.ProductInventory)
        {
            csv.AppendLine($"{product.ProductName},{product.SKU},{product.CategoryName},{product.CurrentStock},{product.MinStockLevel},{product.UnitCost},{product.UnitPrice},{product.TotalValue},{product.StockStatus}");
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}