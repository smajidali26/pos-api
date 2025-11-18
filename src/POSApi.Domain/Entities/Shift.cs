using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Represents a scheduled work shift for an employee
/// </summary>
public class Shift : AggregateRoot
{
    public Guid EmployeeProfileId { get; private set; }
    public Guid? StoreId { get; private set; }
    public DateTime ScheduledStartTime { get; private set; }
    public DateTime ScheduledEndTime { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public ShiftStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // Break Management
    public int ScheduledBreakMinutes { get; private set; }
    public int ActualBreakMinutes { get; private set; }

    // Performance tracking
    public decimal? TotalSales { get; private set; }
    public int? OrdersProcessed { get; private set; }

    // Navigation Properties
    public EmployeeProfile EmployeeProfile { get; set; } = null!;
    public Store? Store { get; set; }
    public ICollection<ShiftAttendance> Attendances { get; set; } = new List<ShiftAttendance>();

    // Constructor for EF Core
    private Shift() { }

    public Shift(
        Guid employeeProfileId,
        DateTime scheduledStartTime,
        DateTime scheduledEndTime,
        int scheduledBreakMinutes = 30,
        Guid? storeId = null,
        string? notes = null)
    {
        if (scheduledEndTime <= scheduledStartTime)
            throw new ArgumentException("End time must be after start time");

        if (scheduledBreakMinutes < 0)
            throw new ArgumentException("Break minutes cannot be negative", nameof(scheduledBreakMinutes));

        EmployeeProfileId = employeeProfileId;
        ScheduledStartTime = scheduledStartTime;
        ScheduledEndTime = scheduledEndTime;
        ScheduledBreakMinutes = scheduledBreakMinutes;
        StoreId = storeId;
        Notes = notes;
        Status = ShiftStatus.Scheduled;
    }

    public void UpdateSchedule(
        DateTime scheduledStartTime,
        DateTime scheduledEndTime,
        int scheduledBreakMinutes,
        string? notes = null)
    {
        if (Status != ShiftStatus.Scheduled)
            throw new InvalidOperationException($"Cannot update schedule for shift with status {Status}");

        if (scheduledEndTime <= scheduledStartTime)
            throw new ArgumentException("End time must be after start time");

        ScheduledStartTime = scheduledStartTime;
        ScheduledEndTime = scheduledEndTime;
        ScheduledBreakMinutes = scheduledBreakMinutes;
        Notes = notes;
    }

    public void ClockIn(DateTime clockInTime)
    {
        if (Status == ShiftStatus.Completed || Status == ShiftStatus.Cancelled)
            throw new InvalidOperationException($"Cannot clock in for shift with status {Status}");

        if (ActualStartTime != null)
            throw new InvalidOperationException("Already clocked in");

        ActualStartTime = clockInTime;
        Status = ShiftStatus.InProgress;
    }

    public void ClockOut(DateTime clockOutTime, int actualBreakMinutes)
    {
        if (Status != ShiftStatus.InProgress)
            throw new InvalidOperationException($"Cannot clock out for shift with status {Status}");

        if (ActualStartTime == null)
            throw new InvalidOperationException("Cannot clock out without clocking in first");

        if (clockOutTime <= ActualStartTime.Value)
            throw new ArgumentException("Clock out time must be after clock in time");

        ActualEndTime = clockOutTime;
        ActualBreakMinutes = actualBreakMinutes;
        Status = ShiftStatus.Completed;
    }

    public void MarkNoShow()
    {
        if (Status != ShiftStatus.Scheduled)
            throw new InvalidOperationException($"Cannot mark no-show for shift with status {Status}");

        Status = ShiftStatus.NoShow;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == ShiftStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed shift");

        Status = ShiftStatus.Cancelled;
        Notes = reason ?? Notes;
    }

    public void UpdatePerformance(decimal totalSales, int ordersProcessed)
    {
        if (Status != ShiftStatus.Completed)
            throw new InvalidOperationException("Can only update performance for completed shifts");

        TotalSales = totalSales;
        OrdersProcessed = ordersProcessed;
    }

    public TimeSpan GetScheduledDuration()
    {
        return ScheduledEndTime - ScheduledStartTime - TimeSpan.FromMinutes(ScheduledBreakMinutes);
    }

    public TimeSpan? GetActualDuration()
    {
        if (ActualStartTime == null || ActualEndTime == null)
            return null;

        return ActualEndTime.Value - ActualStartTime.Value - TimeSpan.FromMinutes(ActualBreakMinutes);
    }

    public bool IsLate()
    {
        if (ActualStartTime == null)
            return false;

        return ActualStartTime.Value > ScheduledStartTime.AddMinutes(15); // 15 minutes grace period
    }

    public bool IsEarly()
    {
        if (ActualStartTime == null)
            return false;

        return ActualStartTime.Value < ScheduledStartTime.AddMinutes(-15);
    }

    public void UpdateBreakTime(int actualBreakMinutes)
    {
        if (Status != ShiftStatus.InProgress && Status != ShiftStatus.Completed)
            throw new InvalidOperationException($"Cannot update break time for shift with status {Status}");

        if (actualBreakMinutes < 0)
            throw new ArgumentException("Break minutes cannot be negative", nameof(actualBreakMinutes));

        ActualBreakMinutes = actualBreakMinutes;
    }

    public void SwapEmployee(Guid newEmployeeProfileId)
    {
        if (Status != ShiftStatus.Scheduled)
            throw new InvalidOperationException($"Cannot swap employee for shift with status {Status}");

        EmployeeProfileId = newEmployeeProfileId;
    }

    public void AddNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note cannot be empty", nameof(note));

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? note
            : $"{Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {note}";
    }
}

public enum ShiftStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}
