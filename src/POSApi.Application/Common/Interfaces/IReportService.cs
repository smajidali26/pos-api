using POSApi.Application.Common.DTOs.Reports;

namespace POSApi.Application.Common.Interfaces;

public interface IReportService
{
    Task<DailySalesReportDto> GenerateDailySalesReportAsync(DateTime reportDate, CancellationToken cancellationToken = default);
    Task<InventoryReportDto> GenerateInventoryReportAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<CustomerAnalyticsReportDto> GenerateCustomerAnalyticsReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportDailySalesReportToCsvAsync(DateTime reportDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportInventoryReportToCsvAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<string> GenerateReportSummaryAsync(DateTime reportDate, CancellationToken cancellationToken = default);
}