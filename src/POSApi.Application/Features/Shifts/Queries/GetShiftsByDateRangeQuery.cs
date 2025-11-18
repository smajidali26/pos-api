using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Queries;

public record GetShiftsByDateRangeQuery : IRequest<List<ShiftDto>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? StoreId { get; init; }
    public Guid? EmployeeProfileId { get; init; }
}

public class GetShiftsByDateRangeQueryHandler : IRequestHandler<GetShiftsByDateRangeQuery, List<ShiftDto>>
{
    private readonly IShiftRepository _repository;

    public GetShiftsByDateRangeQueryHandler(IShiftRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ShiftDto>> Handle(GetShiftsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _repository.GetByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);

        // Apply filters
        if (request.StoreId.HasValue)
            shifts = shifts.Where(s => s.StoreId == request.StoreId.Value).ToList();

        if (request.EmployeeProfileId.HasValue)
            shifts = shifts.Where(s => s.EmployeeProfileId == request.EmployeeProfileId.Value).ToList();

        return shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            EmployeeProfileId = s.EmployeeProfileId,
            EmployeeName = s.EmployeeProfile?.User?.FullName ?? "Unknown",
            StoreId = s.StoreId,
            StoreName = s.Store?.Name,
            ScheduledStartTime = s.ScheduledStartTime,
            ScheduledEndTime = s.ScheduledEndTime,
            ActualStartTime = s.ActualStartTime,
            ActualEndTime = s.ActualEndTime,
            Status = s.Status.ToString(),
            ScheduledBreakMinutes = s.ScheduledBreakMinutes,
            ActualBreakMinutes = s.ActualBreakMinutes,
            TotalSales = s.TotalSales,
            OrdersProcessed = s.OrdersProcessed,
            ScheduledHours = s.GetScheduledDuration().TotalHours,
            ActualHours = s.GetActualDuration()?.TotalHours,
            IsLate = s.IsLate(),
            Notes = s.Notes,
            CreatedAt = s.CreatedAt
        }).OrderBy(s => s.ScheduledStartTime).ToList();
    }
}
