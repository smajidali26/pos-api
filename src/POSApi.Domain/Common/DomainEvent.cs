namespace POSApi.Domain.Common;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;
}