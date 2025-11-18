using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Shifts.Queries;

public record GetShiftsByEmployeeQuery : IRequest<List<ShiftDto>>
{
    public Guid EmployeeProfileId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetShiftsByEmployeeQueryHandler : IRequestHandler<GetShiftsByEmployeeQuery, List<ShiftDto>>
{
    private readonly IShiftRepository _repository;

    public GetShiftsByEmployeeQueryHandler(IShiftRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ShiftDto>> Handle(GetShiftsByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var shifts = await _repository.GetByEmployeeIdAsync(request.EmployeeProfileId, cancellationToken);

        // Apply date filters if provided
        if (request.StartDate.HasValue)
            shifts = shifts.Where(s => s.ScheduledStartTime >= request.StartDate.Value).ToList();

        if (request.EndDate.HasValue)
            shifts = shifts.Where(s => s.ScheduledStartTime <= request.EndDate.Value).ToList();

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
        }).OrderByDescending(s => s.ScheduledStartTime).ToList();
    }
}

public class ShiftDto
{
    public Guid Id { get; set; }
    public Guid EmployeeProfileId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ScheduledBreakMinutes { get; set; }
    public int ActualBreakMinutes { get; set; }
    public decimal? TotalSales { get; set; }
    public int? OrdersProcessed { get; set; }
    public double ScheduledHours { get; set; }
    public double? ActualHours { get; set; }
    public bool IsLate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
