using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class CustomReport : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ReportCategory Category { get; private set; }
    public string SqlQuery { get; private set; } = string.Empty;
    public List<ReportParameter> Parameters { get; private set; } = new();
    public List<ReportColumn> Columns { get; private set; } = new();
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;
    public ReportFrequency DefaultFrequency { get; private set; }
    public List<ReportFormat> SupportedFormats { get; private set; } = new();

    private CustomReport() { } // For EF Core

    public CustomReport(string name, string description, ReportCategory category, string sqlQuery, Guid createdByUserId)
    {
        Name = name;
        Description = description;
        Category = category;
        SqlQuery = sqlQuery;
        CreatedByUserId = createdByUserId;
        IsActive = true;
        DefaultFrequency = ReportFrequency.OnDemand;
        SupportedFormats = new List<ReportFormat> { ReportFormat.PDF, ReportFormat.Excel, ReportFormat.CSV };
    }

    public void AddParameter(string name, string displayName, ParameterType type, bool isRequired = false, string? defaultValue = null)
    {
        var parameter = new ReportParameter(name, displayName, type, isRequired, defaultValue);
        Parameters.Add(parameter);
        SetUpdatedAt();
    }

    public void AddColumn(string name, string displayName, ColumnType type, bool isVisible = true, int? width = null)
    {
        var column = new ReportColumn(name, displayName, type, isVisible, width);
        Columns.Add(column);
        SetUpdatedAt();
    }

    public void UpdateQuery(string sqlQuery)
    {
        SqlQuery = sqlQuery;
        SetUpdatedAt();
    }
}

public class ReportSchedule : AggregateRoot
{
    public Guid ReportId { get; private set; }
    public CustomReport Report { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public ReportFrequency Frequency { get; private set; }
    public DateTime NextRunDate { get; private set; }
    public DateTime? LastRunDate { get; private set; }
    public bool IsActive { get; private set; }
    public List<string> Recipients { get; private set; } = new();
    public ReportFormat Format { get; private set; }
    public Dictionary<string, string> ParameterValues { get; private set; } = new();
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;

    private ReportSchedule() { } // For EF Core

    public ReportSchedule(Guid reportId, string name, ReportFrequency frequency, ReportFormat format, Guid createdByUserId)
    {
        ReportId = reportId;
        Name = name;
        Frequency = frequency;
        Format = format;
        CreatedByUserId = createdByUserId;
        IsActive = true;
        NextRunDate = CalculateNextRunDate();
    }

    public void AddRecipient(string email)
    {
        if (!Recipients.Contains(email))
        {
            Recipients.Add(email);
            SetUpdatedAt();
        }
    }

    public void SetParameterValue(string parameterName, string value)
    {
        ParameterValues[parameterName] = value;
        SetUpdatedAt();
    }

    public void UpdateNextRunDate()
    {
        LastRunDate = DateTime.UtcNow;
        NextRunDate = CalculateNextRunDate();
        SetUpdatedAt();
    }

    private DateTime CalculateNextRunDate()
    {
        var baseDate = LastRunDate ?? DateTime.UtcNow;
        return Frequency switch
        {
            ReportFrequency.Daily => baseDate.AddDays(1),
            ReportFrequency.Weekly => baseDate.AddDays(7),
            ReportFrequency.Monthly => baseDate.AddMonths(1),
            ReportFrequency.Quarterly => baseDate.AddMonths(3),
            ReportFrequency.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddDays(1)
        };
    }
}

public class ReportExecution : AggregateRoot
{
    public Guid ReportId { get; private set; }
    public CustomReport Report { get; private set; } = null!;
    public Guid? ScheduleId { get; private set; }
    public ReportSchedule? Schedule { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public ReportExecutionStatus Status { get; private set; }
    public Dictionary<string, string> ParameterValues { get; private set; } = new();
    public string? FilePath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RecordCount { get; private set; }
    public TimeSpan? ExecutionTime { get; private set; }
    public Guid ExecutedByUserId { get; private set; }
    public User ExecutedBy { get; private set; } = null!;

    private ReportExecution() { } // For EF Core

    public ReportExecution(Guid reportId, Guid executedByUserId, Dictionary<string, string>? parameterValues = null)
    {
        ReportId = reportId;
        ExecutedByUserId = executedByUserId;
        ParameterValues = parameterValues ?? new Dictionary<string, string>();
        StartTime = DateTime.UtcNow;
        Status = ReportExecutionStatus.Running;
    }

    public void Complete(string filePath, int recordCount)
    {
        EndTime = DateTime.UtcNow;
        Status = ReportExecutionStatus.Completed;
        FilePath = filePath;
        RecordCount = recordCount;
        ExecutionTime = EndTime - StartTime;
        SetUpdatedAt();
    }

    public void Fail(string errorMessage)
    {
        EndTime = DateTime.UtcNow;
        Status = ReportExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        ExecutionTime = EndTime - StartTime;
        SetUpdatedAt();
    }
}

public class ReportParameter
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }

    public ReportParameter(string name, string displayName, ParameterType type, bool isRequired = false, string? defaultValue = null)
    {
        Name = name;
        DisplayName = displayName;
        Type = type;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
    }
}

public class ReportColumn
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ColumnType Type { get; set; }
    public bool IsVisible { get; set; }
    public int? Width { get; set; }

    public ReportColumn(string name, string displayName, ColumnType type, bool isVisible = true, int? width = null)
    {
        Name = name;
        DisplayName = displayName;
        Type = type;
        IsVisible = isVisible;
        Width = width;
    }
}

public enum ReportCategory
{
    Sales,
    Inventory,
    Financial,
    Customer,
    Employee,
    Operational,
    Compliance
}

public enum ReportFrequency
{
    OnDemand,
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public enum ReportFormat
{
    PDF,
    Excel,
    CSV,
    JSON,
    XML
}

public enum ParameterType
{
    Text,
    Number,
    Date,
    DateTime,
    Boolean,
    Dropdown,
    MultiSelect
}

public enum ColumnType
{
    Text,
    Number,
    Currency,
    Date,
    DateTime,
    Boolean,
    Percentage
}

public enum ReportExecutionStatus
{
    Running,
    Completed,
    Failed,
    Cancelled
}