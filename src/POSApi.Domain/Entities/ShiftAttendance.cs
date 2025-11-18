using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Tracks clock in/out events during a shift (for break management)
/// </summary>
public class ShiftAttendance : BaseEntity
{
    public Guid ShiftId { get; private set; }
    public AttendanceEventType EventType { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Notes { get; private set; }
    public string? Location { get; private set; } // GPS coordinates or store location
    public string? Device { get; private set; } // Device used for clock in/out

    // Navigation Properties
    public Shift Shift { get; set; } = null!;

    // Constructor for EF Core
    private ShiftAttendance() { }

    public ShiftAttendance(
        Guid shiftId,
        AttendanceEventType eventType,
        DateTime timestamp,
        string? notes = null,
        string? location = null,
        string? device = null)
    {
        ShiftId = shiftId;
        EventType = eventType;
        Timestamp = timestamp;
        Notes = notes;
        Location = location;
        Device = device;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }
}

public enum AttendanceEventType
{
    ClockIn = 0,
    ClockOut = 1,
    BreakStart = 2,
    BreakEnd = 3
}
