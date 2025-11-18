namespace POSApi.Application.Common.DTOs.Reports;

/// <summary>
/// DTO for comprehensive report summary combining sales and inventory data
/// </summary>
public class ReportSummaryDto
{
    public DateTime ReportDate { get; set; }
    public string Summary { get; set; } = string.Empty;
}
