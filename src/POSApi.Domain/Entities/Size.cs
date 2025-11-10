using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class Size : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Size() { } // For EF Core

    public Size(string name, string description)
    {
        Name = name;
        Description = description;
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
