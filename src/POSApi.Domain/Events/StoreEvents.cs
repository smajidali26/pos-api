using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class StoreCreatedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }
    public string StoreCode { get; }

    public StoreCreatedEvent(Guid storeId, string storeName, string storeCode)
    {
        StoreId = storeId;
        StoreName = storeName;
        StoreCode = storeCode;
    }
}

public class StoreActivatedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }

    public StoreActivatedEvent(Guid storeId, string storeName)
    {
        StoreId = storeId;
        StoreName = storeName;
    }
}

public class StoreDeactivatedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }

    public StoreDeactivatedEvent(Guid storeId, string storeName)
    {
        StoreId = storeId;
        StoreName = storeName;
    }
}

public class StoreManagerAssignedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }
    public Guid ManagerUserId { get; }

    public StoreManagerAssignedEvent(Guid storeId, string storeName, Guid managerUserId)
    {
        StoreId = storeId;
        StoreName = storeName;
        ManagerUserId = managerUserId;
    }
}

public class StoreUserAssignedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }
    public Guid UserId { get; }
    public StoreRole Role { get; }

    public StoreUserAssignedEvent(Guid storeId, string storeName, Guid userId, StoreRole role)
    {
        StoreId = storeId;
        StoreName = storeName;
        UserId = userId;
        Role = role;
    }
}

public class StoreUserRemovedEvent : DomainEvent
{
    public Guid StoreId { get; }
    public string StoreName { get; }
    public Guid UserId { get; }

    public StoreUserRemovedEvent(Guid storeId, string storeName, Guid userId)
    {
        StoreId = storeId;
        StoreName = storeName;
        UserId = userId;
    }
}