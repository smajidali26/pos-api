using POSApi.Infrastructure.DTOs.Reports;

namespace POSApi.Infrastructure.Services.Interfaces;

/// <summary>
/// Infrastructure service for generating report data.
/// Note: Export and summary functionality has been moved to Application layer queries (CQRS pattern).
/// </summary>
public interface IInfrastructureReportService
{
    Task<DailySalesReportDto> GenerateDailySalesReportAsync(DateTime reportDate, CancellationToken cancellationToken = default);
    Task<InventoryReportDto> GenerateInventoryReportAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<CustomerAnalyticsReportDto> GenerateCustomerAnalyticsReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}