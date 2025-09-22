using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Notification : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public Guid? RecipientUserId { get; private set; }
    public User? RecipientUser { get; private set; }
    public string? RecipientEmail { get; private set; }
    public string? RecipientPhone { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Dictionary<string, string> Data { get; private set; } = new();
    public NotificationChannel Channel { get; private set; }

    private Notification() { } // For EF Core

    public Notification(string title, string message, NotificationType type, NotificationPriority priority, 
                       NotificationChannel channel, Guid? recipientUserId = null, string? recipientEmail = null, 
                       string? recipientPhone = null)
    {
        Title = title;
        Message = message;
        Type = type;
        Priority = priority;
        Channel = channel;
        RecipientUserId = recipientUserId;
        RecipientEmail = recipientEmail;
        RecipientPhone = recipientPhone;
        Status = NotificationStatus.Pending;
        RetryCount = 0;

        AddDomainEvent(new NotificationCreatedEvent(Id, title, type, priority));
    }

    public void SetRelatedEntity(Guid entityId, string entityType)
    {
        RelatedEntityId = entityId;
        RelatedEntityType = entityType;
        SetUpdatedAt();
    }

    public void AddData(string key, string value)
    {
        Data[key] = value;
        SetUpdatedAt();
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new NotificationSentEvent(Id, Title, Channel));
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Sent)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            SetUpdatedAt();

            AddDomainEvent(new NotificationReadEvent(Id, Title));
        }
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
        SetUpdatedAt();

        AddDomainEvent(new NotificationFailedEvent(Id, Title, errorMessage, RetryCount));
    }

    public void Retry()
    {
        if (Status == NotificationStatus.Failed && RetryCount < 3)
        {
            Status = NotificationStatus.Pending;
            ErrorMessage = null;
            SetUpdatedAt();
        }
    }

    public bool CanRetry => Status == NotificationStatus.Failed && RetryCount < 3;
    public bool IsRead => Status == NotificationStatus.Read;
    public bool IsSent => Status == NotificationStatus.Sent || Status == NotificationStatus.Read;
}

public class NotificationTemplate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsActive { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public List<string> RequiredParameters { get; private set; } = new();
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;

    private NotificationTemplate() { } // For EF Core

    public NotificationTemplate(string name, string subject, string bodyTemplate, NotificationType type, 
                              NotificationChannel channel, Guid createdByUserId, string description = "")
    {
        Name = name;
        Subject = subject;
        BodyTemplate = bodyTemplate;
        Type = type;
        Channel = channel;
        CreatedByUserId = createdByUserId;
        Description = description;
        IsActive = true;

        ExtractRequiredParameters();
    }

    public void UpdateTemplate(string subject, string bodyTemplate, string description = "")
    {
        Subject = subject;
        BodyTemplate = bodyTemplate;
        Description = description;
        SetUpdatedAt();

        ExtractRequiredParameters();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public string GenerateContent(Dictionary<string, string> parameters)
    {
        var content = BodyTemplate;
        foreach (var parameter in parameters)
        {
            content = content.Replace($"{{{{{parameter.Key}}}}}", parameter.Value);
        }
        return content;
    }

    private void ExtractRequiredParameters()
    {
        RequiredParameters.Clear();
        var matches = System.Text.RegularExpressions.Regex.Matches(BodyTemplate, @"\{\{(\w+)\}\}");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var parameter = match.Groups[1].Value;
            if (!RequiredParameters.Contains(parameter))
            {
                RequiredParameters.Add(parameter);
            }
        }
    }
}

public enum NotificationType
{
    LowStock,
    OrderCompleted,
    PaymentReceived,
    ReturnProcessed,
    PromotionStarted,
    PromotionEnding,
    StockCountDue,
    SystemAlert,
    UserAction,
    Security,
    Marketing,
    Reminder
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Read,
    Failed
}

public enum NotificationChannel
{
    InApp,
    Email,
    SMS,
    Push,
    System
}