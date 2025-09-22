using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Category : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { } // For EF Core

    public Category(string name, string description, Guid? parentCategoryId = null)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;

        AddDomainEvent(new CategoryCreatedEvent(Id, name));
    }

    public void UpdateName(string name)
    {
        Name = name;
        SetUpdatedAt();
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}