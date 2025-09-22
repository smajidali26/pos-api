using POSApi.Infrastructure.DTOs.Reports;

namespace POSApi.Infrastructure.Services.Interfaces;

public interface IInfrastructureReportService
{
    Task<DailySalesReportDto> GenerateDailySalesReportAsync(DateTime reportDate, CancellationToken cancellationToken = default);
    Task<InventoryReportDto> GenerateInventoryReportAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<CustomerAnalyticsReportDto> GenerateCustomerAnalyticsReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportDailySalesReportToCsvAsync(DateTime reportDate, CancellationToken cancellationToken = default);
    Task<byte[]> ExportInventoryReportToCsvAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<string> GenerateReportSummaryAsync(DateTime reportDate, CancellationToken cancellationToken = default);
}