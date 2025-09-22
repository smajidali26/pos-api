using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class CustomerCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public CustomerCreatedEvent(Guid customerId, string firstName, string lastName, string email)
    {
        CustomerId = customerId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}