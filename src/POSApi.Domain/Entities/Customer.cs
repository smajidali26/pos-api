using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Customer : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public DateTime? DateOfBirth { get; private set; }
    public decimal LoyaltyPoints { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Customer() { } // For EF Core

    public Customer(string firstName, string lastName, string email, string phoneNumber, 
                   string address, string city, string state, string zipCode, DateTime? dateOfBirth = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        DateOfBirth = dateOfBirth;

        AddDomainEvent(new CustomerCreatedEvent(Id, firstName, lastName, email));
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        SetUpdatedAt();
    }

    public void UpdateAddress(string address, string city, string state, string zipCode)
    {
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        SetUpdatedAt();
    }

    public void AddLoyaltyPoints(decimal points)
    {
        LoyaltyPoints += points;
        SetUpdatedAt();
        
        AddDomainEvent(new LoyaltyPointsEarnedEvent(Id, points, LoyaltyPoints));
    }

    public void RedeemLoyaltyPoints(decimal points)
    {
        if (LoyaltyPoints < points)
        {
            throw new InvalidOperationException($"Insufficient loyalty points. Available: {LoyaltyPoints}, Requested: {points}");
        }

        LoyaltyPoints -= points;
        SetUpdatedAt();
        
        AddDomainEvent(new LoyaltyPointsRedeemedEvent(Id, points, LoyaltyPoints));
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