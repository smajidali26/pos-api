# CQRS Refactoring Guide for Inventory Management

## Overview
This document outlines the CQRS (Command Query Responsibility Segregation) refactoring approach for the inventory management system. The refactoring follows the established MediatR pattern used throughout the POS API.

---

## ✅ Completed Work

### 1. DTOs Created
**File**: `POSApi.Application/Common/DTOs/InventoryDto.cs`

All inventory-related Data Transfer Objects have been created:
- `InventoryMovementDto`
- `LocationDto`
- `ProductLocationDto`
- `StockTransferDto`
- `BatchDto`
- `SerialNumberDto`
- `SerialNumberHistoryDto`
- `StockAlertDto`
- `InventoryValuationDto`
- `InventoryValuationLayerDto`
- Summary DTOs: `InventorySummaryDto`, `StockTransferStatisticsDto`, `StockAlertSummaryDto`, `BatchStatisticsDto`

### 2. AutoMapper Profiles Added
**File**: `POSApi.Application/Common/Mappings/MappingProfile.cs`

Complete mappings for all inventory entities to DTOs:
- InventoryMovement → InventoryMovementDto
- Location → LocationDto
- ProductLocation → ProductLocationDto
- StockTransfer → StockTransferDto
- Batch → BatchDto
- SerialNumber → SerialNumberDto
- SerialNumberHistory → SerialNumberHistoryDto
- StockAlert → StockAlertDto
- InventoryValuation → InventoryValuationDto
- InventoryValuationLayer → InventoryValuationLayerDto

### 3. CQRS Implementation for Inventory Feature
**Location**: `POSApi.Application/Features/Inventory/`

**Commands Created:**
- `RecordInventoryAdjustmentCommand` + Handler + Validator
  - Records manual inventory adjustments
  - Creates InventoryMovement record
  - Updates product stock quantity
  - Returns movement ID

**Queries Created:**
- `GetInventoryMovementsQuery` + Handler
  - Filters: ProductId, LocationId, MovementType, StartDate, EndDate
  - Pagination support
  - Returns `IEnumerable<InventoryMovementDto>`

- `GetStockLevelQuery` + Handler
  - Two modes: Total stock or location-specific
  - Returns `ProductLocationDto`

---

## 📋 Remaining Work

### Controllers to Refactor

#### 1. InventoryController ✅ Partially Done
**Endpoints:**
- ✅ `GET /movements` - Already has query
- ❌ `GET /movements/{id}` - Needs `GetInventoryMovementByIdQuery`
- ✅ `POST /movements/adjustment` - Already has command
- ✅ `GET /stock-level` - Already has query
- ❌ `GET /stock-level/product/{id}/all-locations` - Needs `GetProductStockAllLocationsQuery`
- ❌ `GET /summary/by-location/{id}` - Needs `GetInventorySummaryByLocationQuery`

#### 2. StockTransfersController ❌ Not Started
**Endpoints (9 total):**

**Queries (3):**
- `GET /` - Needs `GetAllStockTransfersQuery`
- `GET /{id}` - Needs `GetStockTransferByIdQuery`
- `GET /statistics` - Needs `GetTransferStatisticsQuery`

**Commands (6):**
- `POST /` - Needs `CreateStockTransferCommand`
- `POST /{id}/approve` - Needs `ApproveStockTransferCommand`
- `POST /{id}/reject` - Needs `RejectStockTransferCommand`
- `POST /{id}/ship` - Needs `ShipStockTransferCommand`
- `POST /{id}/receive` - Needs `ReceiveStockTransferCommand`
- `POST /{id}/cancel` - Needs `CancelStockTransferCommand`

#### 3. LocationsController ❌ Not Started
**Endpoints (7 total):**

**Queries (3):**
- `GET /` - Needs `GetAllLocationsQuery`
- `GET /{id}` - Needs `GetLocationByIdQuery`
- `GET /{id}/products` - Needs `GetLocationProductsQuery`

**Commands (4):**
- `POST /` - Needs `CreateLocationCommand`
- `PUT /{id}` - Needs `UpdateLocationCommand`
- `POST /{id}/activate` - Needs `ActivateLocationCommand`
- `POST /{id}/deactivate` - Needs `DeactivateLocationCommand`

#### 4. BatchesController ❌ Not Started
**Endpoints (7 total):**

**Queries (4):**
- `GET /` - Needs `GetAllBatchesQuery`
- `GET /{id}` - Needs `GetBatchByIdQuery`
- `GET /product/{id}/fifo` - Needs `GetProductBatchesFIFOQuery`
- `GET /expiring` - Needs `GetExpiringBatchesQuery`
- `GET /statistics` - Needs `GetBatchStatisticsQuery`

**Commands (3):**
- `POST /` - Needs `CreateBatchCommand`
- `POST /{id}/recall` - Needs `RecallBatchCommand`
- `POST /{id}/mark-expired` - Needs `MarkBatchExpiredCommand`
- `PUT /{id}/notes` - Needs `UpdateBatchNotesCommand`

#### 5. StockAlertsController ❌ Not Started
**Endpoints (5 total):**

**Queries (2):**
- `GET /` - Needs `GetAllStockAlertsQuery`
- `GET /dashboard-summary` - Needs `GetAlertDashboardSummaryQuery`

**Commands (3):**
- `POST /{id}/acknowledge` - Needs `AcknowledgeAlertCommand`
- `POST /{id}/resolve` - Needs `ResolveAlertCommand`
- `POST /{id}/dismiss` - Needs `DismissAlertCommand`

#### 6. SerialNumbersController ❌ Not Started
**Endpoints (7 total):**

**Queries (3):**
- `GET /` - Needs `GetAllSerialNumbersQuery`
- `GET /{id}` - Needs `GetSerialNumberByIdQuery`
- `GET /warranty-expiring` - Needs `GetExpiringWarrantiesQuery`

**Commands (4):**
- `POST /` - Needs `CreateSerialNumberCommand`
- `POST /{id}/transfer` - Needs `TransferSerialNumberCommand`
- `POST /{id}/mark-defective` - Needs `MarkSerialNumberDefectiveCommand`
- `POST /{id}/repair` - Needs `RepairSerialNumberCommand`

---

## 🔧 Implementation Pattern

### Command Pattern Example
```csharp
// Command
public class CreateStockTransferCommand : ICommand<Guid>
{
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public Guid ToLocationId { get; set; }
    public int RequestedQuantity { get; set; }
    public string? Notes { get; set; }
    public Guid UserId { get; set; } // Set from HttpContext in controller
}

// Validator
public class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FromLocationId).NotEmpty();
        RuleFor(x => x.ToLocationId).NotEmpty()
            .NotEqual(x => x.FromLocationId).WithMessage("From and To locations must be different");
        RuleFor(x => x.RequestedQuantity).GreaterThan(0);
    }
}

// Handler
public class CreateStockTransferCommandHandler : ICommandHandler<CreateStockTransferCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        // Validation
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        // Create entity
        var transfer = new StockTransfer(
            request.ProductId,
            request.FromLocationId,
            request.ToLocationId,
            request.RequestedQuantity,
            request.UserId,
            request.Notes
        );

        // Save
        await _unitOfWork.Context.StockTransfers.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }
}
```

### Query Pattern Example
```csharp
// Query
public class GetAllStockTransfersQuery : IQuery<IEnumerable<StockTransferDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public TransferStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// Handler
public class GetAllStockTransfersQueryHandler : IQueryHandler<GetAllStockTransfersQuery, IEnumerable<StockTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStockTransfersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StockTransferDto>> Handle(GetAllStockTransfersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.StockTransfers.AsQueryable();

        // Apply filters
        if (request.ProductId.HasValue)
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);
        // ... more filters

        var transfers = await query
            .Include(t => t.Product)
            .Include(t => t.FromLocation)
            .Include(t => t.ToLocation)
            .Include(t => t.RequestedBy)
            .OrderByDescending(t => t.RequestedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<StockTransferDto>>(transfers);
    }
}
```

### Controller Pattern Example
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockTransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferDto>>> GetStockTransfers(
        [FromQuery] Guid? productId = null,
        [FromQuery] TransferStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllStockTransfersQuery
        {
            ProductId = productId,
            Status = status,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateStockTransfer(
        [FromBody] CreateStockTransferCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Set UserId from HttpContext
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException());
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetStockTransferById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

---

## 📊 Summary

**Total Endpoints to Refactor**: ~40 endpoints across 6 controllers

**Completed**:
- 3 Inventory queries/commands
- All DTOs
- All AutoMapper profiles

**Remaining**:
- ~37 commands/queries to create
- 6 controllers to refactor

**Estimated Files to Create**: ~75 files
- ~40 command/query files
- ~37 handler files
- Controllers already exist, just need refactoring

---

## 🚀 Next Steps

### Option 1: Incremental Refactoring
Refactor one controller at a time:
1. Create all commands/queries for StockTransfers
2. Refactor StockTransfersController
3. Test endpoints
4. Repeat for remaining controllers

### Option 2: Batch Creation
Create a script to generate boilerplate code for all commands/queries at once, then fill in business logic.

### Option 3: Hybrid Approach (Recommended)
1. Complete StockTransfers (most complex, 9 endpoints) - establishes full pattern
2. Use StockTransfers as template for remaining controllers
3. Focus on high-priority features first (Batches, Alerts, Locations)
4. SerialNumbers last (less frequently used)

---

## ✅ Benefits of This Refactoring

1. **Separation of Concerns**: Business logic separated from HTTP concerns
2. **Testability**: Commands/queries can be unit tested independently
3. **Reusability**: Handlers can be called from multiple places (API, background jobs, etc.)
4. **Validation**: Centralized validation using FluentValidation
5. **Maintainability**: Follows established patterns, easier for team to understand
6. **SOLID Principles**: Single Responsibility, Open/Closed
7. **Clean Architecture**: Application layer independent of infrastructure

---

## 📝 Notes

- All handlers use `IUnitOfWork` for database access
- AutoMapper handles entity-to-DTO conversion
- MediatR automatically discovers and registers all handlers
- FluentValidation validators are auto-discovered
- Controllers become thin orchestration layers
- UserId extracted from HttpContext JWT claims in controllers before sending commands

---

## 🔗 Related Documentation

- `IMPLEMENTATION_SUMMARY.md` - Feature implementation details
- `MIGRATION_GUIDE.md` - Database migration instructions
- Existing CQRS examples in `Features/Vendors/`, `Features/Products/`, etc.
