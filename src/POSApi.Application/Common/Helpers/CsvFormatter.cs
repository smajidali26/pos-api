using System.Text;
using POSApi.Application.Common.DTOs.Reports;

namespace POSApi.Application.Common.Helpers;

/// <summary>
/// Utility class for formatting report DTOs into CSV format
/// </summary>
public static class CsvFormatter
{
    /// <summary>
    /// Converts a daily sales report to CSV format
    /// </summary>
    public static byte[] FormatDailySalesReport(DailySalesReportDto report)
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
            csv.AppendLine($"{EscapeCsvField(product.ProductName)},{EscapeCsvField(product.SKU)},{product.QuantitySold},{product.TotalRevenue}");
        }
        csv.AppendLine();

        // Payment Methods
        csv.AppendLine("Payment Method Breakdown");
        csv.AppendLine("Payment Method,Order Count,Total Amount,Percentage");
        foreach (var payment in report.PaymentMethodBreakdown)
        {
            csv.AppendLine($"{payment.PaymentMethod},{payment.OrderCount},{payment.TotalAmount},{payment.Percentage:F2}%");
        }
        csv.AppendLine();

        // Hourly Sales
        csv.AppendLine("Hourly Sales Breakdown");
        csv.AppendLine("Hour,Order Count,Total Sales");
        foreach (var hourly in report.HourlySalesBreakdown)
        {
            csv.AppendLine($"{hourly.Hour:D2}:00,{hourly.OrderCount},{hourly.TotalSales}");
        }
        csv.AppendLine();

        // Cashier Performance
        csv.AppendLine("Cashier Performance");
        csv.AppendLine("Cashier Name,Orders Processed,Total Sales,Average Order Value");
        foreach (var cashier in report.CashierPerformance)
        {
            csv.AppendLine($"{EscapeCsvField(cashier.CashierName)},{cashier.OrdersProcessed},{cashier.TotalSales},{cashier.AverageOrderValue:F2}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Converts an inventory report to CSV format
    /// </summary>
    public static byte[] FormatInventoryReport(InventoryReportDto report)
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

        // Category Breakdown
        csv.AppendLine("Category Breakdown");
        csv.AppendLine("Category Name,Product Count,Total Stock,Total Value");
        foreach (var category in report.CategoryBreakdown)
        {
            csv.AppendLine($"{EscapeCsvField(category.CategoryName)},{category.ProductCount},{category.TotalStock},{category.TotalValue}");
        }
        csv.AppendLine();

        // Product Details
        csv.AppendLine("Product Inventory");
        csv.AppendLine("Product Name,SKU,Category,Current Stock,Min Stock Level,Unit Cost,Unit Price,Total Value,Status");
        foreach (var product in report.ProductInventory)
        {
            csv.AppendLine($"{EscapeCsvField(product.ProductName)},{EscapeCsvField(product.SKU)},{EscapeCsvField(product.CategoryName)},{product.CurrentStock},{product.MinStockLevel},{product.UnitCost},{product.UnitPrice},{product.TotalValue},{product.StockStatus}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Escapes CSV fields that contain commas, quotes, or newlines
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
