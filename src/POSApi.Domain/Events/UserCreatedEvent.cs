using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Username { get; }
    public string Email { get; }
    public UserRole Role { get; }

    public UserCreatedEvent(Guid userId, string username, string email, UserRole role)
    {
        UserId = userId;
        Username = username;
        Email = email;
        Role = role;
    }
}