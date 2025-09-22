using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class CategoryCreatedEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public string CategoryName { get; }

    public CategoryCreatedEvent(Guid categoryId, string categoryName)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
    }
}