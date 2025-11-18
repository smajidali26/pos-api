using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Extended employee information beyond the basic User entity
/// </summary>
public class EmployeeProfile : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string EmployeeCode { get; private set; }
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public EmploymentStatus Status { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public string? Department { get; private set; }
    public string? JobTitle { get; private set; }
    public Guid? ManagerId { get; private set; }
    public Guid? StoreId { get; private set; }

    // Contact Information
    public string PhoneNumber { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Country { get; private set; }

    // Compensation
    public decimal HourlyRate { get; private set; }
    public bool IsEligibleForCommission { get; private set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public User? Manager { get; set; }
    public Store? Store { get; set; }
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<CommissionTransaction> CommissionTransactions { get; set; } = new List<CommissionTransaction>();
    public ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();

    // Constructor for EF Core
    private EmployeeProfile() { }

    public EmployeeProfile(
        Guid userId,
        string employeeCode,
        DateTime hireDate,
        EmploymentStatus status,
        EmploymentType employmentType,
        string phoneNumber,
        decimal hourlyRate,
        bool isEligibleForCommission = false,
        string? department = null,
        string? jobTitle = null,
        Guid? managerId = null,
        Guid? storeId = null)
    {
        if (hourlyRate < 0)
            throw new ArgumentException("Hourly rate cannot be negative", nameof(hourlyRate));

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentException("Employee code is required", nameof(employeeCode));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        UserId = userId;
        EmployeeCode = employeeCode;
        HireDate = hireDate;
        Status = status;
        EmploymentType = employmentType;
        PhoneNumber = phoneNumber;
        HourlyRate = hourlyRate;
        IsEligibleForCommission = isEligibleForCommission;
        Department = department;
        JobTitle = jobTitle;
        ManagerId = managerId;
        StoreId = storeId;
    }

    public void UpdateProfile(
        string phoneNumber,
        string? department,
        string? jobTitle,
        Guid? managerId,
        Guid? storeId,
        string? address,
        string? city,
        string? state,
        string? zipCode,
        string? country)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        PhoneNumber = phoneNumber;
        Department = department;
        JobTitle = jobTitle;
        ManagerId = managerId;
        StoreId = storeId;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public void UpdateEmergencyContact(string name, string phone)
    {
        EmergencyContactName = name;
        EmergencyContactPhone = phone;
    }

    public void UpdateCompensation(decimal hourlyRate, bool isEligibleForCommission)
    {
        if (hourlyRate < 0)
            throw new ArgumentException("Hourly rate cannot be negative", nameof(hourlyRate));

        HourlyRate = hourlyRate;
        IsEligibleForCommission = isEligibleForCommission;
    }

    public void UpdateEmploymentType(EmploymentType employmentType)
    {
        EmploymentType = employmentType;
    }

    public void Activate()
    {
        if (Status == EmploymentStatus.Terminated)
            throw new InvalidOperationException("Cannot activate a terminated employee");

        Status = EmploymentStatus.Active;
    }

    public void Suspend()
    {
        if (Status == EmploymentStatus.Terminated)
            throw new InvalidOperationException("Cannot suspend a terminated employee");

        Status = EmploymentStatus.Suspended;
    }

    public void PlaceOnLeave()
    {
        if (Status == EmploymentStatus.Terminated)
            throw new InvalidOperationException("Cannot place a terminated employee on leave");

        Status = EmploymentStatus.OnLeave;
    }

    public void Terminate(DateTime terminationDate)
    {
        Status = EmploymentStatus.Terminated;
        TerminationDate = terminationDate;
    }
}

public enum EmploymentStatus
{
    Active = 0,
    Suspended = 1,
    OnLeave = 2,
    Terminated = 3
}

public enum EmploymentType
{
    FullTime = 0,
    PartTime = 1,
    Contract = 2,
    Temporary = 3
}
