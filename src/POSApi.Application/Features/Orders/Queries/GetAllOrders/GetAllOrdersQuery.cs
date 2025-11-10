using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Orders.Queries.GetAllOrders;

public class GetAllOrdersQuery : IQuery<Common.DTOs.PagedResult<OrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // For role-based filtering
    public Guid CurrentUserId { get; set; }
    public UserRole CurrentUserRole { get; set; }

    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}

public class GetAllOrdersQueryHandler : IQueryHandler<GetAllOrdersQuery, Common.DTOs.PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Common.DTOs.PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        Guid? cashierIdFilter = null;

        // Apply role-based filtering
        // Cashiers can only see their own orders
        // Managers and Owners can see all orders
        if (request.CurrentUserRole == UserRole.Cashier)
        {
            cashierIdFilter = request.CurrentUserId;
        }

        // Get orders with pagination and filters
        var (orders, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(
            request.Skip,
            request.Take,
            cashierIdFilter,
            request.CustomerId,
            request.Status,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new Common.DTOs.PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }
}
