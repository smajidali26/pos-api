# Employee Management Implementation Guide
**POS System - Feature 4**

## Document Information
- **Version**: 1.0
- **Date**: 2025-11-18
- **Status**: In Progress

---

## Table of Contents
1. [Overview](#overview)
2. [Domain Entities](#domain-entities)
3. [CQRS Implementation](#cqrs-implementation)
4. [Repository Interfaces](#repository-interfaces)
5. [API Endpoints](#api-endpoints)
6. [Background Jobs](#background-jobs)
7. [Frontend Components](#frontend-components)
8. [Implementation Status](#implementation-status)

---

## Overview

### Features Implemented:
1. **Employee Profiles** - Extended employee information beyond User entity
2. **Shift Management** - Scheduling, clock in/out, attendance tracking
3. **Commission Tracking** - Flexible commission rules and earnings
4. **Performance Metrics** - Comprehensive performance analysis

### Technology Stack:
- **.NET 9** - Backend API
- **React 19.1.1** - Frontend UI
- **CQRS with MediatR** - Command/Query separation
- **Entity Framework Core** - ORM
- **Clean Architecture** - Domain-driven design

---

## Domain Entities

### 1. EmployeeProfile
**Purpose**: Extended employee information beyond the basic User entity

**Properties:**
```csharp
- UserId (Guid) - Link to User entity
- EmployeeCode (string) - Unique employee identifier
- HireDate (DateTime) - Date of employment start
- TerminationDate (DateTime?) - Optional termination date
- Status (EmploymentStatus) - Active, Suspended, OnLeave, Terminated
- EmploymentType (EmploymentType) - FullTime, PartTime, Contract, Temporary
- Department (string?) - Department name
- JobTitle (string?) - Job title/position
- ManagerId (Guid?) - Manager's EmployeeProfile ID
- StoreId (Guid?) - Assigned store
- PhoneNumber (string) - Contact phone
- EmergencyContactName (string?) - Emergency contact
- EmergencyContactPhone (string?) - Emergency phone
- Address/City/State/ZipCode/Country - Full address
- HourlyRate (decimal) - Hourly compensation rate
- IsEligibleForCommission (bool) - Commission eligibility
```

**Key Methods:**
- `UpdateProfile()` - Update contact and assignment info
- `UpdateEmergencyContact()` - Update emergency contact
- `UpdateCompensation()` - Update pay rate and commission eligibility
- `Activate()` / `Suspend()` / `PlaceOnLeave()` / `Terminate()` - Status changes

---

### 2. Shift
**Purpose**: Scheduled work shift with attendance tracking

**Properties:**
```csharp
- EmployeeProfileId (Guid) - Employee assignment
- StoreId (Guid?) - Store location
- ScheduledStartTime (DateTime) - Planned start
- ScheduledEndTime (DateTime) - Planned end
- ActualStartTime (DateTime?) - Actual clock in
- ActualEndTime (DateTime?) - Actual clock out
- Status (ShiftStatus) - Scheduled, InProgress, Completed, Cancelled, NoShow
- ScheduledBreakMinutes (int) - Planned break duration
- ActualBreakMinutes (int) - Actual break taken
- TotalSales (decimal?) - Sales during shift
- OrdersProcessed (int?) - Orders handled
- Notes (string?) - Shift notes
```

**Key Methods:**
- `UpdateSchedule()` - Modify schedule before shift starts
- `ClockIn()` - Mark shift started
- `ClockOut()` - Mark shift completed
- `UpdatePerformance()` - Record sales metrics
- `MarkNoShow()` - Mark employee didn't show
- `Cancel()` - Cancel scheduled shift
- `GetScheduledDuration()` - Calculate planned hours
- `GetActualDuration()` - Calculate worked hours
- `IsLate()` / `IsEarly()` - Check punctuality (15 min grace)

---

### 3. ShiftAttendance
**Purpose**: Detailed attendance event log (clock in/out, breaks)

**Properties:**
```csharp
- ShiftId (Guid) - Parent shift
- EventType (AttendanceEventType) - ClockIn, ClockOut, BreakStart, BreakEnd
- Timestamp (DateTime) - When event occurred
- Notes (string?) - Event notes
- Location (string?) - GPS coordinates or location
- Device (string?) - Device used for clocking
```

---

### 4. Commission
**Purpose**: Defines commission rules for employees

**Properties:**
```csharp
- Name (string) - Commission rule name
- Description (string) - Rule description
- CommissionType (CommissionType) - Percentage, FixedAmount, Tiered
- CommissionBasis (CommissionBasis) - TotalSale, GrossProfit, ItemsSold, Category
- Rate (decimal) - Percentage or fixed amount
- MinimumSaleAmount (decimal?) - Minimum order amount
- MaximumCommission (decimal?) - Commission cap
- IsActive (bool) - Rule active status
- EffectiveFrom (DateTime) - Start date
- EffectiveTo (DateTime?) - Optional end date
- EmployeeProfileId (Guid?) - Specific employee (optional)
- ProductId (Guid?) - Specific product (optional)
- CategoryId (Guid?) - Specific category (optional)
- Role (string?) - Role-based (Cashier, Manager, etc.)
```

**Key Methods:**
- `UpdateRate()` - Change commission rate
- `Activate()` / `Deactivate()` - Enable/disable rule
- `IsApplicableToOrder()` - Check if commission applies
- `IsApplicableToProduct()` - Check product applicability
- `CalculateCommission()` - Calculate commission amount

**Example Commission Rules:**
1. **General Cashier Commission**: 2% on all sales over $100
2. **Product-Specific**: $5 per unit for Product X
3. **Category Commission**: 5% on Electronics category
4. **High-Value Sales**: 10% on orders over $1000

---

### 5. CommissionTransaction
**Purpose**: Individual commission earnings record

**Properties:**
```csharp
- EmployeeProfileId (Guid) - Employee who earned
- CommissionId (Guid) - Commission rule applied
- OrderId (Guid) - Order that earned commission
- OrderItemId (Guid?) - Specific item (optional)
- TransactionDate (DateTime) - When earned
- SaleAmount (decimal) - Sale amount
- CommissionAmount (decimal) - Commission earned
- Status (CommissionStatus) - Pending, Approved, Paid, Voided
- PaidDate (DateTime?) - Payment date
- PaymentReference (string?) - Payment reference number
- Notes (string?) - Transaction notes
```

**Key Methods:**
- `Approve()` - Approve for payment
- `Pay()` - Mark as paid
- `Void()` - Cancel commission
- `Adjust()` - Modify amount before payment

---

### 6. PerformanceMetric
**Purpose**: Tracks employee performance for a period

**Properties:**
```csharp
- EmployeeProfileId (Guid) - Employee
- PeriodStart/PeriodEnd (DateTime) - Time period
- PeriodType (MetricPeriodType) - Daily, Weekly, Monthly, Quarterly, Yearly

// Sales Metrics
- TotalSales (decimal) - Total sales amount
- OrdersProcessed (int) - Number of orders
- AverageOrderValue (decimal) - Avg order size
- ItemsSold (int) - Total items sold

// Performance Metrics
- CommissionEarned (decimal?) - Total commission
- RefundsProcessed (int) - Refund count
- RefundAmount (decimal) - Refund total
- RefundRate (decimal) - Refund percentage

// Attendance Metrics
- ScheduledShifts (int) - Shifts scheduled
- CompletedShifts (int) - Shifts worked
- MissedShifts (int) - No-shows
- LateArrivals (int) - Late count
- TotalHoursWorked (decimal) - Hours worked
- TotalBreakHours (decimal) - Break time

// Customer Satisfaction
- CustomerRatingCount (int?) - Rating count
- AverageCustomerRating (decimal?) - Avg rating (out of 5)

// Calculated Score
- PerformanceScore (decimal) - Score out of 100
- PerformanceGrade (string?) - A, B, C, D, F
```

**Performance Score Calculation (100 points):**
- **Sales Performance**: 40 points (based on sales per hour)
- **Attendance**: 30 points (completion rate, -10 for late arrivals)
- **Customer Satisfaction**: 20 points (based on rating out of 5)
- **Low Refund Rate**: 10 points (penalty for high refund rate)

**Grading Scale:**
- A: 90-100 (Excellent)
- B: 80-89 (Good)
- C: 70-79 (Satisfactory)
- D: 60-69 (Needs Improvement)
- F: 0-59 (Unsatisfactory)

**Key Methods:**
- `UpdateSalesMetrics()` - Update sales data
- `UpdateAttendanceMetrics()` - Update attendance data
- `UpdateCommissionEarned()` - Set commission total
- `UpdateCustomerSatisfaction()` - Set rating data
- `CalculatePerformanceScore()` - Compute score and grade
- `GetAttendanceRate()` - Calculate attendance percentage
- `GetSalesPerHour()` - Calculate sales efficiency
- `IsExcellentPerformer()` - Check if score >= 90
- `NeedsImprovement()` - Check if score < 70

---

## CQRS Implementation

### Employee Profile Commands (4 files created)
1. **CreateEmployeeProfileCommand** - Create new employee profile
2. **UpdateEmployeeProfileCommand** - Update profile information
3. **UpdateCompensationCommand** - Update pay rate and commission eligibility
4. **UpdateEmploymentStatusCommand** - Change status (Active/Suspended/OnLeave/Terminated)

### Employee Profile Queries (2 files created)
1. **GetAllEmployeeProfilesQuery** - List all profiles with filters (status, store, manager)
2. **GetEmployeeProfileByIdQuery** - Get detailed profile with user, manager, store info

### Shift Commands (10 planned)
1. **CreateShiftCommand** ✅ - Schedule new shift
2. **UpdateShiftCommand** - Modify scheduled shift
3. **ClockInCommand** ✅ - Employee clock in
4. **ClockOutCommand** ✅ - Employee clock out
5. **RecordBreakCommand** - Record break start/end
6. **MarkNoShowCommand** - Mark employee didn't show
7. **CancelShiftCommand** - Cancel scheduled shift
8. **UpdateShiftPerformanceCommand** - Record sales metrics
9. **BulkCreateShiftsCommand** - Create multiple shifts (weekly schedule)
10. **SwapShiftCommand** - Swap shifts between employees

### Shift Queries (5 planned)
1. **GetShiftsByEmployeeQuery** - Get employee's shifts
2. **GetShiftsByDateRangeQuery** - Get shifts for date range
3. **GetActiveShiftsQuery** - Get currently in-progress shifts
4. **GetUpcomingShiftsQuery** - Get scheduled future shifts
5. **GetShiftAttendanceQuery** - Get attendance details for shift

### Commission Commands (6 planned)
1. **CreateCommissionCommand** - Define new commission rule
2. **UpdateCommissionCommand** - Modify commission rule
3. **ActivateCommissionCommand** - Enable rule
4. **DeactivateCommissionCommand** - Disable rule
5. **CalculateOrderCommissionsCommand** - Calculate commissions for order
6. **ProcessCommissionPaymentCommand** - Mark commissions as paid

### Commission Queries (4 planned)
1. **GetAllCommissionsQuery** - List all commission rules
2. **GetActiveCommissionsQuery** - Get active rules
3. **GetCommissionTransactionsQuery** - Get employee's commission history
4. **GetPendingCommissionsQuery** - Get unpaid commissions

### Performance Metric Commands (3 planned)
1. **CalculatePerformanceMetricsCommand** - Calculate metrics for period
2. **UpdateCustomerRatingCommand** - Update customer satisfaction
3. **RecalculateAllMetricsCommand** - Batch recalculation

### Performance Metric Queries (4 planned)
1. **GetEmployeePerformanceQuery** - Get employee's metrics
2. **GetTopPerformersQuery** - Leaderboard
3. **GetPerformanceComparisonQuery** - Compare employees
4. **GetPerformanceTrendsQuery** - Historical trends

---

## Repository Interfaces

### Required Repositories (to be created in Infrastructure layer):

```csharp
// POSApi.Domain/Repositories/IEmployeeProfileRepository.cs
public interface IEmployeeProfileRepository : IRepository<EmployeeProfile>
{
    Task<EmployeeProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetAllWithUserAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<List<EmployeeProfile>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
}

// POSApi.Domain/Repositories/IShiftRepository.cs
public interface IShiftRepository : IRepository<Shift>
{
    Task<List<Shift>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<Shift>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<List<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);
    Task<List<Shift>> GetUpcomingShiftsAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<Shift?> GetByIdWithAttendanceAsync(Guid id, CancellationToken cancellationToken = default);
}

// POSApi.Domain/Repositories/IShiftAttendanceRepository.cs
public interface IShiftAttendanceRepository : IRepository<ShiftAttendance>
{
    Task<List<ShiftAttendance>> GetByShiftIdAsync(Guid shiftId, CancellationToken cancellationToken = default);
}

// POSApi.Domain/Repositories/ICommissionRepository.cs
public interface ICommissionRepository : IRepository<Commission>
{
    Task<List<Commission>> GetActiveCommissionsAsync(CancellationToken cancellationToken = default);
    Task<List<Commission>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<Commission>> GetApplicableCommissionsAsync(Guid? employeeProfileId, string? role, CancellationToken cancellationToken = default);
}

// POSApi.Domain/Repositories/ICommissionTransactionRepository.cs
public interface ICommissionTransactionRepository : IRepository<CommissionTransaction>
{
    Task<List<CommissionTransaction>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<List<CommissionTransaction>> GetPendingCommissionsAsync(CancellationToken cancellationToken = default);
    Task<List<CommissionTransaction>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalEarnedAsync(Guid employeeProfileId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

// POSApi.Domain/Repositories/IPerformanceMetricRepository.cs
public interface IPerformanceMetricRepository : IRepository<PerformanceMetric>
{
    Task<List<PerformanceMetric>> GetByEmployeeIdAsync(Guid employeeProfileId, CancellationToken cancellationToken = default);
    Task<PerformanceMetric?> GetByPeriodAsync(Guid employeeProfileId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
    Task<List<PerformanceMetric>> GetTopPerformersAsync(DateTime periodStart, DateTime periodEnd, int count, CancellationToken cancellationToken = default);
}
```

---

## API Endpoints

### Employee Profiles Controller (8 endpoints)
```
GET    /api/employee-profiles
GET    /api/employee-profiles/{id}
GET    /api/employee-profiles/user/{userId}
POST   /api/employee-profiles
PUT    /api/employee-profiles/{id}
PUT    /api/employee-profiles/{id}/compensation
PUT    /api/employee-profiles/{id}/status
DELETE /api/employee-profiles/{id}
```

### Shifts Controller (12 endpoints)
```
GET    /api/shifts
GET    /api/shifts/{id}
GET    /api/shifts/employee/{employeeId}
GET    /api/shifts/date-range
GET    /api/shifts/active
GET    /api/shifts/upcoming/{employeeId}
POST   /api/shifts
POST   /api/shifts/bulk
PUT    /api/shifts/{id}
POST   /api/shifts/{id}/clock-in
POST   /api/shifts/{id}/clock-out
POST   /api/shifts/{id}/cancel
```

### Commissions Controller (10 endpoints)
```
GET    /api/commissions
GET    /api/commissions/{id}
GET    /api/commissions/active
GET    /api/commissions/employee/{employeeId}
POST   /api/commissions
PUT    /api/commissions/{id}
PUT    /api/commissions/{id}/activate
PUT    /api/commissions/{id}/deactivate
GET    /api/commissions/transactions/{employeeId}
POST   /api/commissions/calculate/{orderId}
```

### Performance Metrics Controller (8 endpoints)
```
GET    /api/performance/employee/{employeeId}
GET    /api/performance/{id}
GET    /api/performance/period
GET    /api/performance/top-performers
GET    /api/performance/comparison
GET    /api/performance/trends/{employeeId}
POST   /api/performance/calculate
POST   /api/performance/recalculate-all
```

**Total New Endpoints**: 38

---

## Background Jobs

### Job 1: Daily Performance Metrics Calculation
**Schedule**: Daily at 3 AM UTC
**Purpose**: Calculate daily performance metrics for all active employees

```csharp
public async Task CalculateDailyPerformanceMetricsAsync()
{
    var yesterday = DateTime.UtcNow.AddDays(-1).Date;
    var activeEmployees = await _employeeProfileRepository.GetActiveEmployeesAsync();

    foreach (var employee in activeEmployees)
    {
        // Get shifts for yesterday
        var shifts = await _shiftRepository.GetByEmployeeAndDateAsync(employee.Id, yesterday);

        // Get orders processed
        var orders = await _orderRepository.GetByEmployeeAndDateAsync(employee.UserId, yesterday);

        // Calculate metrics
        var metric = new PerformanceMetric(employee.Id, yesterday, yesterday.AddDays(1), MetricPeriodType.Daily);

        // Update sales metrics
        var totalSales = orders.Sum(o => o.TotalAmount);
        var ordersProcessed = orders.Count;
        var itemsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);
        var refunds = orders.Where(o => o.Status == OrderStatus.Refunded);

        metric.UpdateSalesMetrics(totalSales, ordersProcessed, itemsSold, refunds.Count(), refunds.Sum(r => r.TotalAmount));

        // Update attendance metrics
        var scheduledShifts = shifts.Count;
        var completedShifts = shifts.Count(s => s.Status == ShiftStatus.Completed);
        var missedShifts = shifts.Count(s => s.Status == ShiftStatus.NoShow);
        var lateArrivals = shifts.Count(s => s.IsLate());
        var totalHours = shifts.Sum(s => (decimal)(s.GetActualDuration()?.TotalHours ?? 0));
        var breakHours = shifts.Sum(s => (decimal)s.ActualBreakMinutes / 60);

        metric.UpdateAttendanceMetrics(scheduledShifts, completedShifts, missedShifts, lateArrivals, totalHours, breakHours);

        // Update commission
        var commissions = await _commissionTransactionRepository.GetByEmployeeAndDateAsync(employee.Id, yesterday);
        metric.UpdateCommissionEarned(commissions.Sum(c => c.CommissionAmount));

        // Calculate final score
        metric.CalculatePerformanceScore();

        await _performanceMetricRepository.AddAsync(metric);
    }

    await _unitOfWork.SaveChangesAsync();
}
```

### Job 2: Weekly Commission Processing
**Schedule**: Weekly on Monday at 1 AM UTC
**Purpose**: Approve pending commissions for payment

```csharp
public async Task ProcessWeeklyCommissionsAsync()
{
    var pendingCommissions = await _commissionTransactionRepository.GetPendingCommissionsAsync();

    foreach (var commission in pendingCommissions)
    {
        // Auto-approve commissions older than 7 days
        if ((DateTime.UtcNow - commission.TransactionDate).TotalDays >= 7)
        {
            commission.Approve();
        }
    }

    await _unitOfWork.SaveChangesAsync();
}
```

### Job 3: Monthly Performance Reports
**Schedule**: Monthly on 1st at 6 AM UTC
**Purpose**: Generate monthly performance summaries

### Job 4: Shift Reminder Notifications
**Schedule**: Hourly
**Purpose**: Send reminders for upcoming shifts (1 hour before)

---

## Frontend Components

### Pages (4)
1. **EmployeeManagementPage** - Main dashboard
2. **EmployeeProfilePage** - Individual employee details
3. **ShiftSchedulePage** - Shift calendar and management
4. **CommissionReportsPage** - Commission tracking and reports

### Components (20+)
1. **EmployeeList** - List all employees
2. **EmployeeCard** - Employee summary card
3. **CreateEmployeeModal** - Create employee profile form
4. **EditEmployeeModal** - Edit employee details
5. **EmployeeDetailsCard** - Detailed employee info
6. **ShiftCalendar** - Calendar view of shifts
7. **CreateShiftModal** - Schedule shift form
8. **ShiftCard** - Shift summary
9. **ClockInOutWidget** - Quick clock in/out
10. **AttendanceHistoryTable** - Attendance records
11. **CommissionRulesTable** - Commission rules list
12. **CreateCommissionModal** - Define commission rule
13. **CommissionTransactionsTable** - Commission history
14. **PerformanceMetricsCard** - Performance summary
15. **PerformanceChart** - Performance visualization
16. **LeaderboardWidget** - Top performers
17. **PerformanceTrendChart** - Historical performance
18. **ShiftSwapModal** - Swap shift between employees
19. **EmployeeStatusBadge** - Status indicator
20. **HourlyRateInput** - Pay rate editor

### Redux Slices (4)
1. **employeeProfilesSlice** - Employee state management
2. **shiftsSlice** - Shift state management
3. **commissionsSlice** - Commission state management
4. **performanceSlice** - Performance metrics state

### Services (4)
1. **employeeProfileService** - API calls for profiles
2. **shiftService** - API calls for shifts
3. **commissionService** - API calls for commissions
4. **performanceService** - API calls for metrics

---

## Implementation Status

### ✅ Completed
- [x] Domain entities design (6 entities)
- [x] EmployeeProfile CQRS commands (4 files)
- [x] EmployeeProfile CQRS queries (2 files)
- [x] Shift commands - CreateShift, ClockIn, ClockOut (3 files)

### 🔄 In Progress
- [ ] Remaining Shift commands (7 files)
- [ ] Shift queries (5 files)
- [ ] Commission CQRS implementation (10 files)
- [ ] Performance Metrics CQRS (7 files)
- [ ] Repository implementations (6 repositories)
- [ ] Database configurations (EF Core)
- [ ] API Controllers (4 controllers)
- [ ] Background jobs (4 jobs)
- [ ] Frontend components (20+ components)

### 📋 Pending
- [ ] Database migration
- [ ] Unit tests
- [ ] Integration tests
- [ ] Documentation
- [ ] Deployment guide

---

## Next Steps

1. **Complete CQRS Implementation**
   - Finish Shift commands/queries
   - Implement Commission commands/queries
   - Implement Performance Metrics commands/queries

2. **Repository Layer**
   - Create repository interfaces
   - Implement repositories in Infrastructure
   - Add Entity Framework configurations

3. **API Controllers**
   - Create 4 controllers
   - Add authorization
   - Add Swagger documentation

4. **Background Jobs**
   - Implement 4 Hangfire jobs
   - Schedule recurring jobs
   - Test job execution

5. **Frontend Implementation**
   - Create Redux slices and sagas
   - Build React components
   - Implement API services
   - Add routing

6. **Testing & Documentation**
   - Write unit tests
   - Create integration tests
   - Update system documentation
   - Create user guide

---

## Database Schema

### New Tables (6)
1. **EmployeeProfiles** - Employee extended information
2. **Shifts** - Shift schedules
3. **ShiftAttendances** - Attendance events
4. **Commissions** - Commission rules
5. **CommissionTransactions** - Commission earnings
6. **PerformanceMetrics** - Performance records

### Foreign Keys
- EmployeeProfiles.UserId → Users.Id
- EmployeeProfiles.ManagerId → EmployeeProfiles.Id
- EmployeeProfiles.StoreId → Stores.Id
- Shifts.EmployeeProfileId → EmployeeProfiles.Id
- Shifts.StoreId → Stores.Id
- ShiftAttendances.ShiftId → Shifts.Id
- Commissions.EmployeeProfileId → EmployeeProfiles.Id
- Commissions.ProductId → Products.Id
- Commissions.CategoryId → Categories.Id
- CommissionTransactions.EmployeeProfileId → EmployeeProfiles.Id
- CommissionTransactions.CommissionId → Commissions.Id
- CommissionTransactions.OrderId → Orders.Id
- CommissionTransactions.OrderItemId → OrderItems.Id
- PerformanceMetrics.EmployeeProfileId → EmployeeProfiles.Id

---

**Document Version**: 1.0
**Last Updated**: 2025-11-18
**Next Review**: When implementation is 50% complete
