using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services.Export;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly PosDbContext _context;
    private readonly IExportService _pdfExportService;
    private readonly CsvExportService _csvExportService;

    public ExportController(
        PosDbContext context,
        IExportService pdfExportService,
        CsvExportService csvExportService)
    {
        _context = context;
        _pdfExportService = pdfExportService;
        _csvExportService = csvExportService;
    }

    /// <summary>
    /// Export products to PDF
    /// </summary>
    [HttpGet("products/pdf")]
    public async Task<IActionResult> ExportProductsToPdf()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Select(p => new
            {
                p.Name,
                p.SKU,
                Category = p.Category != null ? p.Category.Name : "N/A",
                Size = p.Size != null ? p.Size.Name : "N/A",
                p.Stock,
                p.CostPrice,
                p.SellingPrice,
                p.IsActive
            })
            .ToListAsync();

        var columns = new Dictionary<string, string>
        {
            { "Name", "Product Name" },
            { "SKU", "SKU" },
            { "Category", "Category" },
            { "Size", "Size" },
            { "Stock", "Stock" },
            { "CostPrice", "Cost Price" },
            { "SellingPrice", "Selling Price" },
            { "IsActive", "Active" }
        };

        var pdfBytes = _pdfExportService.ExportToPdf(products, "Products Report", columns);

        return File(pdfBytes, "application/pdf", $"Products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Export products to CSV
    /// </summary>
    [HttpGet("products/csv")]
    public async Task<IActionResult> ExportProductsToCsv()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Select(p => new
            {
                p.Name,
                p.SKU,
                Category = p.Category != null ? p.Category.Name : "N/A",
                Size = p.Size != null ? p.Size.Name : "N/A",
                p.Stock,
                p.CostPrice,
                p.SellingPrice,
                p.IsActive
            })
            .ToListAsync();

        var csvBytes = _csvExportService.ExportToCsv(products);

        return File(csvBytes, "text/csv", $"Products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Export orders to PDF
    /// </summary>
    [HttpGet("orders/pdf")]
    public async Task<IActionResult> ExportOrdersToPdf(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        var orders = await query
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.OrderNumber,
                o.OrderDate,
                Customer = o.Customer != null ? o.Customer.Name : "Walk-in",
                o.Status,
                o.Subtotal,
                o.TaxAmount,
                o.TotalAmount
            })
            .ToListAsync();

        var columns = new Dictionary<string, string>
        {
            { "OrderNumber", "Order #" },
            { "OrderDate", "Date" },
            { "Customer", "Customer" },
            { "Status", "Status" },
            { "Subtotal", "Subtotal" },
            { "TaxAmount", "Tax" },
            { "TotalAmount", "Total" }
        };

        var pdfBytes = _pdfExportService.ExportToPdf(orders, "Orders Report", columns);

        return File(pdfBytes, "application/pdf", $"Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Export orders to CSV
    /// </summary>
    [HttpGet("orders/csv")]
    public async Task<IActionResult> ExportOrdersToCsv(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        var orders = await query
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.OrderNumber,
                o.OrderDate,
                Customer = o.Customer != null ? o.Customer.Name : "Walk-in",
                o.Status,
                o.Subtotal,
                o.TaxAmount,
                o.TotalAmount
            })
            .ToListAsync();

        var csvBytes = _csvExportService.ExportToCsv(orders);

        return File(csvBytes, "text/csv", $"Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Export sales forecasts to PDF
    /// </summary>
    [HttpGet("forecasts/pdf")]
    public async Task<IActionResult> ExportForecastsToPdf([FromQuery] Guid? productId = null)
    {
        var query = _context.SalesForecasts
            .Include(sf => sf.Product)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sf => sf.ProductId == productId.Value);

        var forecasts = await query
            .Where(sf => sf.ForecastDate >= DateTime.UtcNow.Date)
            .OrderBy(sf => sf.ForecastDate)
            .Select(sf => new
            {
                Product = sf.Product.Name,
                sf.ForecastDate,
                sf.PredictedQuantity,
                sf.ForecastMethod,
                sf.ConfidenceLevel
            })
            .ToListAsync();

        var columns = new Dictionary<string, string>
        {
            { "Product", "Product" },
            { "ForecastDate", "Forecast Date" },
            { "PredictedQuantity", "Predicted Quantity" },
            { "ForecastMethod", "Method" },
            { "ConfidenceLevel", "Confidence %" }
        };

        var pdfBytes = _pdfExportService.ExportToPdf(forecasts, "Sales Forecasts Report", columns);

        return File(pdfBytes, "application/pdf", $"Forecasts_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Export sales forecasts to CSV
    /// </summary>
    [HttpGet("forecasts/csv")]
    public async Task<IActionResult> ExportForecastsToCsv([FromQuery] Guid? productId = null)
    {
        var query = _context.SalesForecasts
            .Include(sf => sf.Product)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sf => sf.ProductId == productId.Value);

        var forecasts = await query
            .Where(sf => sf.ForecastDate >= DateTime.UtcNow.Date)
            .OrderBy(sf => sf.ForecastDate)
            .Select(sf => new
            {
                Product = sf.Product.Name,
                sf.ForecastDate,
                sf.PredictedQuantity,
                sf.ForecastMethod,
                sf.ConfidenceLevel
            })
            .ToListAsync();

        var csvBytes = _csvExportService.ExportToCsv(forecasts);

        return File(csvBytes, "text/csv", $"Forecasts_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Export loyalty members to PDF
    /// </summary>
    [HttpGet("loyalty/members/pdf")]
    public async Task<IActionResult> ExportLoyaltyMembersToPdf()
    {
        var members = await _context.CustomerLoyalties
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .Select(cl => new
            {
                Customer = cl.Customer.Name,
                Email = cl.Customer.Email,
                Tier = cl.CurrentTier != null ? cl.CurrentTier.Name : "None",
                cl.CurrentPoints,
                cl.LifetimePoints,
                cl.LifetimeSpend,
                cl.JoinDate
            })
            .ToListAsync();

        var columns = new Dictionary<string, string>
        {
            { "Customer", "Customer Name" },
            { "Email", "Email" },
            { "Tier", "Tier" },
            { "CurrentPoints", "Current Points" },
            { "LifetimePoints", "Lifetime Points" },
            { "LifetimeSpend", "Lifetime Spend" },
            { "JoinDate", "Join Date" }
        };

        var pdfBytes = _pdfExportService.ExportToPdf(members, "Loyalty Members Report", columns);

        return File(pdfBytes, "application/pdf", $"LoyaltyMembers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Export loyalty members to CSV
    /// </summary>
    [HttpGet("loyalty/members/csv")]
    public async Task<IActionResult> ExportLoyaltyMembersToCsv()
    {
        var members = await _context.CustomerLoyalties
            .Include(cl => cl.Customer)
            .Include(cl => cl.CurrentTier)
            .Select(cl => new
            {
                Customer = cl.Customer.Name,
                Email = cl.Customer.Email,
                Tier = cl.CurrentTier != null ? cl.CurrentTier.Name : "None",
                cl.CurrentPoints,
                cl.LifetimePoints,
                cl.LifetimeSpend,
                cl.JoinDate
            })
            .ToListAsync();

        var csvBytes = _csvExportService.ExportToCsv(members);

        return File(csvBytes, "text/csv", $"LoyaltyMembers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Export ABC classification to PDF
    /// </summary>
    [HttpGet("abc-classification/pdf")]
    public async Task<IActionResult> ExportABCClassificationToPdf()
    {
        var classifications = await _context.ProductABCClassifications
            .Include(c => c.Product)
            .OrderBy(c => c.Classification)
            .ThenByDescending(c => c.ContributionPercentage)
            .Select(c => new
            {
                Product = c.Product.Name,
                SKU = c.Product.SKU,
                c.Classification,
                c.ContributionPercentage,
                c.CumulativePercentage,
                c.AnalysisDate
            })
            .ToListAsync();

        var columns = new Dictionary<string, string>
        {
            { "Product", "Product Name" },
            { "SKU", "SKU" },
            { "Classification", "ABC Class" },
            { "ContributionPercentage", "Contribution %" },
            { "CumulativePercentage", "Cumulative %" },
            { "AnalysisDate", "Analysis Date" }
        };

        var pdfBytes = _pdfExportService.ExportToPdf(classifications, "ABC Classification Report", columns);

        return File(pdfBytes, "application/pdf", $"ABC_Classification_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Export ABC classification to CSV
    /// </summary>
    [HttpGet("abc-classification/csv")]
    public async Task<IActionResult> ExportABCClassificationToCsv()
    {
        var classifications = await _context.ProductABCClassifications
            .Include(c => c.Product)
            .OrderBy(c => c.Classification)
            .ThenByDescending(c => c.ContributionPercentage)
            .Select(c => new
            {
                Product = c.Product.Name,
                SKU = c.Product.SKU,
                c.Classification,
                c.ContributionPercentage,
                c.CumulativePercentage,
                c.AnalysisDate
            })
            .ToListAsync();

        var csvBytes = _csvExportService.ExportToCsv(classifications);

        return File(csvBytes, "text/csv", $"ABC_Classification_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
