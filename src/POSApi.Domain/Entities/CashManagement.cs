using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class CashDrawer : AggregateRoot
{
    public string DrawerNumber { get; private set; } = string.Empty;
    public Guid StoreId { get; private set; }
    public Store Store { get; private set; } = null!;
    public Guid? CurrentShiftId { get; private set; }
    public Shift? CurrentShift { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public CashDrawerStatus Status { get; private set; }
    public DateTime? LastOpenedAt { get; private set; }
    public DateTime? LastClosedAt { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<CashMovement> CashMovements { get; private set; } = new List<CashMovement>();
    public ICollection<Shift> Shifts { get; private set; } = new List<Shift>();

    private CashDrawer() { } // For EF Core

    public CashDrawer(string drawerNumber, Guid storeId, decimal openingBalance = 0)
    {
        DrawerNumber = drawerNumber;
        StoreId = storeId;
        OpeningBalance = openingBalance;
        CurrentBalance = openingBalance;
        Status = CashDrawerStatus.Closed;
        IsActive = true;

        AddDomainEvent(new CashDrawerCreatedEvent(Id, drawerNumber, storeId));
    }

    public void Open(Guid userId, decimal startingCash)
    {
        if (Status != CashDrawerStatus.Closed)
        {
            throw new InvalidOperationException($"Cannot open drawer in {Status} status");
        }

        Status = CashDrawerStatus.Open;
        LastOpenedAt = DateTime.UtcNow;
        CurrentBalance = startingCash;
        SetUpdatedAt();

        AddDomainEvent(new CashDrawerOpenedEvent(Id, DrawerNumber, userId, startingCash));
    }

    public void Close(Guid userId, decimal endingCash)
    {
        if (Status != CashDrawerStatus.Open)
        {
            throw new InvalidOperationException($"Cannot close drawer in {Status} status");
        }

        Status = CashDrawerStatus.Closed;
        LastClosedAt = DateTime.UtcNow;
        var variance = endingCash - CurrentBalance;
        SetUpdatedAt();

        AddDomainEvent(new CashDrawerClosedEvent(Id, DrawerNumber, userId, endingCash, CurrentBalance, variance));
        
        CurrentBalance = endingCash;
    }

    public void RecordCashMovement(CashMovementType type, decimal amount, Guid userId, string reason, string? referenceNumber = null)
    {
        if (Status != CashDrawerStatus.Open)
        {
            throw new InvalidOperationException("Drawer must be open to record cash movements");
        }

        var movement = new CashMovement(Id, type, amount, userId, reason, referenceNumber);
        CashMovements.Add(movement);

        CurrentBalance += type switch
        {
            CashMovementType.CashIn or CashMovementType.Sale => amount,
            CashMovementType.CashOut or CashMovementType.Refund => -amount,
            _ => 0
        };

        SetUpdatedAt();
        AddDomainEvent(new CashMovementRecordedEvent(Id, type, amount, CurrentBalance));
    }

    public decimal CalculateExpectedBalance()
    {
        return OpeningBalance + CashMovements
            .Where(cm => cm.Type == CashMovementType.CashIn || cm.Type == CashMovementType.Sale)
            .Sum(cm => cm.Amount) - CashMovements
            .Where(cm => cm.Type == CashMovementType.CashOut || cm.Type == CashMovementType.Refund)
            .Sum(cm => cm.Amount);
    }
}

public class Shift : AggregateRoot
{
    public string ShiftNumber { get; private set; } = string.Empty;
    public Guid CashDrawerId { get; private set; }
    public CashDrawer CashDrawer { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public decimal StartingCash { get; private set; }
    public decimal? EndingCash { get; private set; }
    public decimal? CashVariance { get; private set; }
    public ShiftStatus Status { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public ICollection<TillCount> TillCounts { get; private set; } = new List<TillCount>();

    private Shift() { } // For EF Core

    public Shift(string shiftNumber, Guid cashDrawerId, Guid userId, decimal startingCash)
    {
        ShiftNumber = shiftNumber;
        CashDrawerId = cashDrawerId;
        UserId = userId;
        StartingCash = startingCash;
        StartTime = DateTime.UtcNow;
        Status = ShiftStatus.Active;

        AddDomainEvent(new ShiftStartedEvent(Id, shiftNumber, userId, startingCash));
    }

    public void EndShift(decimal endingCash, string notes = "")
    {
        if (Status != ShiftStatus.Active)
        {
            throw new InvalidOperationException($"Cannot end shift in {Status} status");
        }

        EndTime = DateTime.UtcNow;
        EndingCash = endingCash;
        CashVariance = endingCash - CalculateExpectedCash();
        Status = ShiftStatus.Completed;
        Notes = notes;
        SetUpdatedAt();

        AddDomainEvent(new ShiftEndedEvent(Id, ShiftNumber, UserId, endingCash, CashVariance.Value));
    }

    public void AddTillCount(Dictionary<CashDenomination, int> denominations, Guid countedByUserId)
    {
        if (Status != ShiftStatus.Active)
        {
            throw new InvalidOperationException("Can only count till during active shift");
        }

        var tillCount = new TillCount(Id, denominations, countedByUserId);
        TillCounts.Add(tillCount);
        SetUpdatedAt();
    }

    private decimal CalculateExpectedCash()
    {
        var salesTotal = Orders.Where(o => o.PaymentMethod == PaymentMethod.Cash).Sum(o => o.TotalAmount);
        return StartingCash + salesTotal;
    }

    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
    public decimal TotalSales => Orders.Sum(o => o.TotalAmount);
    public int TransactionCount => Orders.Count;
}

public class CashMovement : BaseEntity
{
    public Guid CashDrawerId { get; private set; }
    public CashDrawer CashDrawer { get; private set; } = null!;
    public CashMovementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public string? ReferenceNumber { get; private set; }
    public DateTime MovementTime { get; private set; }

    private CashMovement() { } // For EF Core

    public CashMovement(Guid cashDrawerId, CashMovementType type, decimal amount, Guid userId, string reason, string? referenceNumber = null)
    {
        CashDrawerId = cashDrawerId;
        Type = type;
        Amount = amount;
        UserId = userId;
        Reason = reason;
        ReferenceNumber = referenceNumber;
        MovementTime = DateTime.UtcNow;
    }
}

public class TillCount : BaseEntity
{
    public Guid ShiftId { get; private set; }
    public Shift Shift { get; private set; } = null!;
    public Guid CountedByUserId { get; private set; }
    public User CountedBy { get; private set; } = null!;
    public DateTime CountTime { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Dictionary<CashDenomination, int> Denominations { get; private set; } = new();

    private TillCount() { } // For EF Core

    public TillCount(Guid shiftId, Dictionary<CashDenomination, int> denominations, Guid countedByUserId)
    {
        ShiftId = shiftId;
        Denominations = denominations;
        CountedByUserId = countedByUserId;
        CountTime = DateTime.UtcNow;
        TotalAmount = CalculateTotal();
    }

    private decimal CalculateTotal()
    {
        return Denominations.Sum(d => d.Key.Value * d.Value);
    }
}

public class CashDenomination
{
    public decimal Value { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CashDenominationType Type { get; private set; }

    public CashDenomination(decimal value, string name, CashDenominationType type)
    {
        Value = value;
        Name = name;
        Type = type;
    }

    // Common denominations
    public static readonly CashDenomination OneDollar = new(1.00m, "$1", CashDenominationType.Bill);
    public static readonly CashDenomination FiveDollar = new(5.00m, "$5", CashDenominationType.Bill);
    public static readonly CashDenomination TenDollar = new(10.00m, "$10", CashDenominationType.Bill);
    public static readonly CashDenomination TwentyDollar = new(20.00m, "$20", CashDenominationType.Bill);
    public static readonly CashDenomination Quarter = new(0.25m, "Quarter", CashDenominationType.Coin);
    public static readonly CashDenomination Dime = new(0.10m, "Dime", CashDenominationType.Coin);
    public static readonly CashDenomination Nickel = new(0.05m, "Nickel", CashDenominationType.Coin);
    public static readonly CashDenomination Penny = new(0.01m, "Penny", CashDenominationType.Coin);
}

public enum CashDrawerStatus
{
    Closed,
    Open,
    Maintenance
}

public enum ShiftStatus
{
    Active,
    Completed,
    Cancelled
}

public enum CashMovementType
{
    CashIn,     // Adding cash to drawer
    CashOut,    // Removing cash from drawer
    Sale,       // Sale transaction
    Refund,     // Refund transaction
    Drop,       // Cash drop to safe
    Pickup,     // Cash pickup from safe
    Adjustment  // Manual adjustment
}

public enum CashDenominationType
{
    Bill,
    Coin
}