using FluentValidation;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using System.Globalization;

namespace POSApi.Application.Features.Analytics.Queries.GetSalesAnalytics;

public class GetSalesAnalyticsQuery : IQuery<SalesAnalyticsDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? StoreId { get; set; }
    public string GroupBy { get; set; } = "day"; // day, week, month
}

public class GetSalesAnalyticsQueryValidator : AbstractValidator<GetSalesAnalyticsQuery>
{
    public GetSalesAnalyticsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.GroupBy)
            .Must(g => new[] { "day", "week", "month" }.Contains(g.ToLower()))
            .WithMessage("GroupBy must be 'day', 'week', or 'month'");
    }
}

public class GetSalesAnalyticsQueryHandler : IQueryHandler<GetSalesAnalyticsQuery, SalesAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSalesAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesAnalyticsDto> Handle(GetSalesAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _unitOfWork.Context.Set<Order>()
            .Where(o => o.OrderDate >= request.StartDate && o.OrderDate <= request.EndDate && o.Status != OrderStatus.Cancelled);

        if (request.StoreId.HasValue)
        {
            baseQuery = baseQuery.Where(o => o.StoreId == request.StoreId.Value);
        }

        var orders = await baseQuery
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(o => o.Customer)
            .ToListAsync(cancellationToken);

        // Calculate summary metrics
        var totalSales = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var totalCost = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Product.Cost * oi.Quantity);
        var totalProfit = totalSales - totalCost;
        var profitMargin = totalSales > 0 ? (totalProfit / totalSales) * 100 : 0;
        var totalItemsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity);

        // Daily sales
        var dailySales = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Sales = g.Sum(o => o.TotalAmount),
                Orders = g.Count(),
                Profit = g.Sum(o => o.TotalAmount) - g.SelectMany(o => o.OrderItems).Sum(oi => oi.Product.Cost * oi.Quantity),
                AvgOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Weekly sales
        var weeklySales = orders
            .GroupBy(o => new { Year = o.OrderDate.Year, Week = ISOWeek.GetWeekOfYear(o.OrderDate) })
            .Select(g => new WeeklySalesDto
            {
                Year = g.Key.Year,
                Week = g.Key.Week,
                StartDate = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                EndDate = ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Sunday),
                Sales = g.Sum(o => o.TotalAmount),
                Orders = g.Count(),
                Profit = g.Sum(o => o.TotalAmount) - g.SelectMany(o => o.OrderItems).Sum(oi => oi.Product.Cost * oi.Quantity)
            })
            .OrderBy(w => w.Year).ThenBy(w => w.Week)
            .ToList();

        // Monthly sales
        var monthlySales = orders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new MonthlySalesDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                Sales = g.Sum(o => o.TotalAmount),
                Orders = g.Count(),
                Profit = g.Sum(o => o.TotalAmount) - g.SelectMany(o => o.OrderItems).Sum(oi => oi.Product.Cost * oi.Quantity),
                GrowthRate = 0 // Will calculate below
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        // Calculate growth rate for monthly sales
        for (int i = 1; i < monthlySales.Count; i++)
        {
            var current = monthlySales[i];
            var previous = monthlySales[i - 1];
            current.GrowthRate = previous.Sales > 0 ? ((current.Sales - previous.Sales) / previous.Sales) * 100 : 0;
        }

        // Top products
        var topProducts = orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopProductPerformanceDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                SKU = g.Key.SKU,
                Revenue = g.Sum(oi => oi.Price * oi.Quantity),
                QuantitySold = (int)g.Sum(oi => oi.Quantity),
                Profit = g.Sum(oi => (oi.Price - oi.Product.Cost) * oi.Quantity),
                ProfitMargin = g.Sum(oi => oi.Price * oi.Quantity) > 0 ?
                    (g.Sum(oi => (oi.Price - oi.Product.Cost) * oi.Quantity) / g.Sum(oi => oi.Price * oi.Quantity)) * 100 : 0,
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(p => p.Revenue)
            .Take(20)
            .ToList();

        // Top categories
        var topCategories = orders
            .SelectMany(o => o.OrderItems)
            .Where(oi => oi.Product.Category != null)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category!.Name })
            .Select(g => new TopCategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Revenue = g.Sum(oi => oi.Price * oi.Quantity),
                ItemsSold = (int)g.Sum(oi => oi.Quantity),
                Profit = g.Sum(oi => (oi.Price - oi.Product.Cost) * oi.Quantity),
                ContributionPercentage = 0
            })
            .OrderByDescending(c => c.Revenue)
            .Take(10)
            .ToList();

        var categoryTotal = topCategories.Sum(c => c.Revenue);
        foreach (var cat in topCategories)
        {
            cat.ContributionPercentage = categoryTotal > 0 ? Math.Round((cat.Revenue / categoryTotal) * 100, 2) : 0;
        }

        // Top customers
        var topCustomers = orders
            .Where(o => o.Customer != null)
            .GroupBy(o => new { o.CustomerId, o.Customer!.Name, o.Customer.Email })
            .Select(g => new TopCustomerPerformanceDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                Email = g.Key.Email,
                TotalPurchases = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                AvgOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0,
                LastPurchaseDate = g.Max(o => o.OrderDate)
            })
            .OrderByDescending(c => c.TotalPurchases)
            .Take(20)
            .ToList();

        // Sales by hour of day
        var salesByHour = orders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new HourlyPerformanceDto
            {
                Hour = g.Key,
                AverageSales = g.Sum(o => o.TotalAmount) / orders.GroupBy(o => o.OrderDate.Date).Count(),
                AverageOrders = g.Count() / Math.Max(1, orders.GroupBy(o => o.OrderDate.Date).Count())
            })
            .OrderBy(h => h.Hour)
            .ToList();

        // Sales by day of week
        var salesByDayOfWeek = orders
            .GroupBy(o => o.OrderDate.DayOfWeek)
            .Select(g => new DayOfWeekPerformanceDto
            {
                DayOfWeek = (int)g.Key,
                DayName = g.Key.ToString(),
                AverageSales = g.Sum(o => o.TotalAmount) / orders.GroupBy(o => o.OrderDate.Date).Count(),
                AverageOrders = g.Count() / Math.Max(1, orders.GroupBy(o => o.OrderDate.Date).Count())
            })
            .OrderBy(d => d.DayOfWeek)
            .ToList();

        // Calculate growth metrics
        var periodLength = (request.EndDate - request.StartDate).Days;
        var comparisonStartDate = request.StartDate.AddDays(-periodLength);
        var comparisonEndDate = request.StartDate.AddDays(-1);

        var comparisonSales = await _unitOfWork.Context.Set<Order>()
            .Where(o => o.OrderDate >= comparisonStartDate && o.OrderDate <= comparisonEndDate && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var percentageChange = comparisonSales > 0 ? ((totalSales - comparisonSales) / comparisonSales) * 100 : 0;
        var growthRate = periodLength > 0 ? percentageChange / periodLength : 0;

        return new SalesAnalyticsDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StoreId = request.StoreId,
            TotalSales = Math.Round(totalSales, 2),
            TotalOrders = totalOrders,
            AvgOrderValue = Math.Round(avgOrderValue, 2),
            TotalProfit = Math.Round(totalProfit, 2),
            ProfitMargin = Math.Round(profitMargin, 2),
            TotalCost = Math.Round(totalCost, 2),
            TotalItemsSold = totalItemsSold,
            DailySales = dailySales,
            WeeklySales = weeklySales,
            MonthlySales = monthlySales,
            TopProducts = topProducts,
            TopCategories = topCategories,
            TopCustomers = topCustomers,
            SalesByHour = salesByHour,
            SalesByDayOfWeek = salesByDayOfWeek,
            GrowthRate = Math.Round(growthRate, 2),
            ComparisonPeriodSales = Math.Round(comparisonSales, 2),
            PercentageChange = Math.Round(percentageChange, 2)
        };
    }
}
