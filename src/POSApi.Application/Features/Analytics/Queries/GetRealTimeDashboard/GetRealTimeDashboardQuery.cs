using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Analytics.Queries.GetRealTimeDashboard;

public class GetRealTimeDashboardQuery : IQuery<RealTimeDashboardDto>
{
    public Guid? StoreId { get; set; }

    public GetRealTimeDashboardQuery(Guid? storeId = null)
    {
        StoreId = storeId;
    }
}

public class GetRealTimeDashboardQueryHandler : IQueryHandler<GetRealTimeDashboardQuery, RealTimeDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRealTimeDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RealTimeDashboardDto> Handle(GetRealTimeDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var lastWeek = today.AddDays(-7);
        var lastMonth = today.AddMonths(-1);
        var lastYear = today.AddYears(-1);

        // Base query for orders
        var baseQuery = _unitOfWork.Context.Set<Order>()
            .Where(o => o.Status != OrderStatus.Cancelled);

        if (request.StoreId.HasValue)
        {
            baseQuery = baseQuery.Where(o => o.StoreId == request.StoreId.Value);
        }

        // Today's metrics
        var todayOrders = await baseQuery
            .Where(o => o.OrderDate.Date == today)
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .ToListAsync(cancellationToken);

        var todaySales = todayOrders.Sum(o => o.TotalAmount);
        var todayOrderCount = todayOrders.Count;
        var todayCustomerCount = todayOrders.Select(o => o.CustomerId).Distinct().Count();
        var todayAvgOrderValue = todayOrderCount > 0 ? todaySales / todayOrderCount : 0;
        var todayProfit = todayOrders.SelectMany(o => o.OrderItems)
            .Sum(oi => (oi.Price - oi.Product.Cost) * oi.Quantity);
        var todayProfitMargin = todaySales > 0 ? (todayProfit / todaySales) * 100 : 0;

        // Yesterday's sales for comparison
        var yesterdaySales = await baseQuery
            .Where(o => o.OrderDate.Date == yesterday)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        // Week-over-week comparison
        var thisWeekSales = await baseQuery
            .Where(o => o.OrderDate.Date >= lastWeek && o.OrderDate.Date <= today)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var twoWeeksAgo = lastWeek.AddDays(-7);
        var lastWeekSales = await baseQuery
            .Where(o => o.OrderDate.Date >= twoWeeksAgo && o.OrderDate.Date < lastWeek)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var wowChange = lastWeekSales > 0 ? ((thisWeekSales - lastWeekSales) / lastWeekSales) * 100 : 0;

        // Month-over-month comparison
        var thisMonthSales = await baseQuery
            .Where(o => o.OrderDate.Date >= lastMonth && o.OrderDate.Date <= today)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var twoMonthsAgo = lastMonth.AddMonths(-1);
        var lastMonthSales = await baseQuery
            .Where(o => o.OrderDate.Date >= twoMonthsAgo && o.OrderDate.Date < lastMonth)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var momChange = lastMonthSales > 0 ? ((thisMonthSales - lastMonthSales) / lastMonthSales) * 100 : 0;

        // Year-over-year comparison
        var thisYearSales = await baseQuery
            .Where(o => o.OrderDate.Date >= lastYear && o.OrderDate.Date <= today)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var twoYearsAgo = lastYear.AddYears(-1);
        var lastYearSales = await baseQuery
            .Where(o => o.OrderDate.Date >= twoYearsAgo && o.OrderDate.Date < lastYear)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var yoyChange = lastYearSales > 0 ? ((thisYearSales - lastYearSales) / lastYearSales) * 100 : 0;

        // Hourly sales breakdown for today
        var hourlySales = todayOrders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new HourlySalesDto
            {
                Hour = g.Key,
                Sales = g.Sum(o => o.TotalAmount),
                Orders = g.Count(),
                AvgOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            })
            .OrderBy(h => h.Hour)
            .ToList();

        // Top products today
        var topProducts = todayOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                SKU = g.Key.SKU,
                Revenue = g.Sum(oi => oi.Price * oi.Quantity),
                QuantitySold = (int)g.Sum(oi => oi.Quantity),
                Profit = g.Sum(oi => (oi.Price - oi.Product.Cost) * oi.Quantity)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        // Top categories today
        var topCategories = todayOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category.Name })
            .Select(g => new TopCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Revenue = g.Sum(oi => oi.Price * oi.Quantity),
                ItemsSold = (int)g.Sum(oi => oi.Quantity),
                ContributionPercentage = 0 // Will calculate below
            })
            .OrderByDescending(c => c.Revenue)
            .Take(10)
            .ToList();

        var totalCategorySales = topCategories.Sum(c => c.Revenue);
        foreach (var category in topCategories)
        {
            category.ContributionPercentage = totalCategorySales > 0
                ? Math.Round((category.Revenue / totalCategorySales) * 100, 2)
                : 0;
        }

        // Top customers today
        var topCustomers = todayOrders
            .Where(o => o.Customer != null)
            .GroupBy(o => new { o.CustomerId, CustomerName = o.Customer!.Name })
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                TotalPurchases = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                AvgOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            })
            .OrderByDescending(c => c.TotalPurchases)
            .Take(10)
            .ToList();

        // Payment methods breakdown
        var paymentMethods = await _unitOfWork.Context.Set<Payment>()
            .Where(p => p.PaymentDate.Date == today)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodBreakdownDto
            {
                PaymentMethod = g.Key.ToString(),
                Amount = g.Sum(p => p.Amount),
                TransactionCount = g.Count(),
                Percentage = 0 // Will calculate below
            })
            .ToListAsync(cancellationToken);

        var totalPayments = paymentMethods.Sum(p => p.Amount);
        foreach (var method in paymentMethods)
        {
            method.Percentage = totalPayments > 0
                ? Math.Round((method.Amount / totalPayments) * 100, 2)
                : 0;
        }

        // Inventory status
        var productsQuery = _unitOfWork.Context.Set<Product>()
            .Where(p => p.IsActive);

        var lowStockCount = await productsQuery
            .CountAsync(p => p.StockQuantity <= p.MinStockLevel, cancellationToken);

        var outOfStockCount = await productsQuery
            .CountAsync(p => p.StockQuantity <= 0, cancellationToken);

        var totalInventoryValue = await productsQuery
            .SumAsync(p => p.StockQuantity * p.Cost, cancellationToken);

        return new RealTimeDashboardDto
        {
            TodaySales = Math.Round(todaySales, 2),
            TodayOrders = todayOrderCount,
            TodayCustomers = todayCustomerCount,
            TodayAvgOrderValue = Math.Round(todayAvgOrderValue, 2),
            TodayProfit = Math.Round(todayProfit, 2),
            TodayProfitMargin = Math.Round(todayProfitMargin, 2),
            YesterdaySales = Math.Round(yesterdaySales, 2),
            WoWChange = Math.Round(wowChange, 2),
            MoMChange = Math.Round(momChange, 2),
            YoYChange = Math.Round(yoyChange, 2),
            HourlySales = hourlySales,
            TopProducts = topProducts,
            TopCategories = topCategories,
            TopCustomers = topCustomers,
            SalesByPaymentMethod = paymentMethods,
            LowStockItems = lowStockCount,
            OutOfStockItems = outOfStockCount,
            TotalInventoryValue = Math.Round(totalInventoryValue, 2),
            LastUpdated = DateTime.UtcNow
        };
    }
}
