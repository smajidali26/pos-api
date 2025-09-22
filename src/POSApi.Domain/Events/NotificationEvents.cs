using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class NotificationCreatedEvent : DomainEvent
{
    public Guid NotificationId { get; }
    public string Title { get; }
    public NotificationType Type { get; }
    public NotificationPriority Priority { get; }

    public NotificationCreatedEvent(Guid notificationId, string title, NotificationType type, NotificationPriority priority)
    {
        NotificationId = notificationId;
        Title = title;
        Type = type;
        Priority = priority;
    }
}

public class NotificationSentEvent : DomainEvent
{
    public Guid NotificationId { get; }
    public string Title { get; }
    public NotificationChannel Channel { get; }

    public NotificationSentEvent(Guid notificationId, string title, NotificationChannel channel)
    {
        NotificationId = notificationId;
        Title = title;
        Channel = channel;
    }
}

public class NotificationReadEvent : DomainEvent
{
    public Guid NotificationId { get; }
    public string Title { get; }

    public NotificationReadEvent(Guid notificationId, string title)
    {
        NotificationId = notificationId;
        Title = title;
    }
}

public class NotificationFailedEvent : DomainEvent
{
    public Guid NotificationId { get; }
    public string Title { get; }
    public string ErrorMessage { get; }
    public int RetryCount { get; }

    public NotificationFailedEvent(Guid notificationId, string title, string errorMessage, int retryCount)
    {
        NotificationId = notificationId;
        Title = title;
        ErrorMessage = errorMessage;
        RetryCount = retryCount;
    }
}