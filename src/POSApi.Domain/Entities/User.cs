using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class User : AggregateRoot
{
    public string Username { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginDate { get; private set; }
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private User() { } // For EF Core

    public User(string username, string firstName, string lastName, string email, 
               string passwordHash, UserRole role)
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;

        AddDomainEvent(new UserCreatedEvent(Id, username, email, role));
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        SetUpdatedAt();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
        SetUpdatedAt();
    }

    public void RecordLogin()
    {
        LastLoginDate = DateTime.UtcNow;
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

public enum UserRole
{
    Cashier,
    Manager,
    Administrator,
    Owner
}