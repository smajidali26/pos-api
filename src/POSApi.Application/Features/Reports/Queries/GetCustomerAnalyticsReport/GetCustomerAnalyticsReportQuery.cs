using POSApi.Application.Common.DTOs.Reports;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Reports.Queries.GetCustomerAnalyticsReport;

public class GetCustomerAnalyticsReportQuery : IQuery<CustomerAnalyticsReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetCustomerAnalyticsReportQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate.Date;
        EndDate = endDate.Date.AddDays(1); // Include the entire end date
    }
}

public class GetCustomerAnalyticsReportQueryHandler : IQueryHandler<GetCustomerAnalyticsReportQuery, CustomerAnalyticsReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerAnalyticsReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerAnalyticsReportDto> Handle(GetCustomerAnalyticsReportQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        var customerList = customers.ToList();

        var orders = await _unitOfWork.Orders.GetOrdersByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();

        var newCustomers = customerList.Where(c => c.CreatedAt >= request.StartDate && c.CreatedAt < request.EndDate).ToList();
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
            TotalPointsRedeemed = CalculateTotalPointsRedeemed(customerList),
            PointsOutstanding = customerList.Sum(c => c.LoyaltyPoints),
            AveragePointsPerCustomer = customerList.Any() ? customerList.Average(c => c.LoyaltyPoints) : 0
        };

        return report;
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

        var totalRevenue = customers.Sum(c => c.TotalSpent);
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

    private static decimal CalculateTotalPointsRedeemed(List<Customer> customers)
    {
        // This would require tracking point redemption history
        // For now, we'll estimate based on difference between issued and current points
        return 0; // This should be implemented with proper point history tracking
    }
}