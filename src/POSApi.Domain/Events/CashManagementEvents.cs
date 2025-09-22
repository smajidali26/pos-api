using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class CashDrawerCreatedEvent : DomainEvent
{
    public Guid CashDrawerId { get; }
    public string DrawerNumber { get; }
    public Guid StoreId { get; }

    public CashDrawerCreatedEvent(Guid cashDrawerId, string drawerNumber, Guid storeId)
    {
        CashDrawerId = cashDrawerId;
        DrawerNumber = drawerNumber;
        StoreId = storeId;
    }
}

public class CashDrawerOpenedEvent : DomainEvent
{
    public Guid CashDrawerId { get; }
    public string DrawerNumber { get; }
    public Guid UserId { get; }
    public decimal StartingCash { get; }

    public CashDrawerOpenedEvent(Guid cashDrawerId, string drawerNumber, Guid userId, decimal startingCash)
    {
        CashDrawerId = cashDrawerId;
        DrawerNumber = drawerNumber;
        UserId = userId;
        StartingCash = startingCash;
    }
}

public class CashDrawerClosedEvent : DomainEvent
{
    public Guid CashDrawerId { get; }
    public string DrawerNumber { get; }
    public Guid UserId { get; }
    public decimal EndingCash { get; }
    public decimal ExpectedCash { get; }
    public decimal Variance { get; }

    public CashDrawerClosedEvent(Guid cashDrawerId, string drawerNumber, Guid userId, decimal endingCash, decimal expectedCash, decimal variance)
    {
        CashDrawerId = cashDrawerId;
        DrawerNumber = drawerNumber;
        UserId = userId;
        EndingCash = endingCash;
        ExpectedCash = expectedCash;
        Variance = variance;
    }
}

public class CashMovementRecordedEvent : DomainEvent
{
    public Guid CashDrawerId { get; }
    public CashMovementType MovementType { get; }
    public decimal Amount { get; }
    public decimal NewBalance { get; }

    public CashMovementRecordedEvent(Guid cashDrawerId, CashMovementType movementType, decimal amount, decimal newBalance)
    {
        CashDrawerId = cashDrawerId;
        MovementType = movementType;
        Amount = amount;
        NewBalance = newBalance;
    }
}

public class ShiftStartedEvent : DomainEvent
{
    public Guid ShiftId { get; }
    public string ShiftNumber { get; }
    public Guid UserId { get; }
    public decimal StartingCash { get; }

    public ShiftStartedEvent(Guid shiftId, string shiftNumber, Guid userId, decimal startingCash)
    {
        ShiftId = shiftId;
        ShiftNumber = shiftNumber;
        UserId = userId;
        StartingCash = startingCash;
    }
}

public class ShiftEndedEvent : DomainEvent
{
    public Guid ShiftId { get; }
    public string ShiftNumber { get; }
    public Guid UserId { get; }
    public decimal EndingCash { get; }
    public decimal Variance { get; }

    public ShiftEndedEvent(Guid shiftId, string shiftNumber, Guid userId, decimal endingCash, decimal variance)
    {
        ShiftId = shiftId;
        ShiftNumber = shiftNumber;
        UserId = userId;
        EndingCash = endingCash;
        Variance = variance;
    }
}