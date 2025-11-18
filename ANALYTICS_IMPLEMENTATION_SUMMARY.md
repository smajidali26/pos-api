# Advanced Reporting & Analytics - Implementation Summary

## Overview
Comprehensive analytics and reporting system with real-time dashboards, sales forecasting, ABC inventory classification, and inventory turnover analysis.

---

## Part 1: Domain Entities

### File: `D:\Majid\POS\pos-api\src\POSApi.Domain\Entities\AdvancedAnalytics.cs`

#### Entities Created:
1. **SalesForecast** (AggregateRoot)
   - Properties: ForecastDate, ProductId, StoreId, PredictedQuantity, PredictedRevenue, ConfidenceLevel, ForecastMethod
   - Methods: `UpdateForecast()`, `AdjustConfidence()`, `AddMetadata()`
   - Purpose: Store sales predictions for products

2. **ProductABCClassification** (AggregateRoot)
   - Properties: ProductId, StoreId, Classification (A/B/C), AnnualVolume, AnnualRevenue, ContributionPercentage, Rank, CumulativePercentage
   - Methods: `Reclassify()`, `UpdateMetrics()`
   - Purpose: ABC analysis based on Pareto principle (80-15-5 rule)

3. **InventoryTurnover** (BaseEntity)
   - Properties: ProductId, StoreId, TurnoverRatio, DaysToSell, AverageCOGS, AverageInventoryValue, Classification
   - Methods: `UpdateMetrics()`
   - Purpose: Track inventory turnover metrics

#### Enums:
- **ForecastMethod**: LinearRegression, MovingAverage, ExponentialSmoothing, SeasonalDecomposition, WeightedAverage, AutoSelected
- **ABCClass**: A (top 20%, 80% revenue), B (next 30%, 15% revenue), C (remaining 50%, 5% revenue)
- **TurnoverClassification**: Fast (>12/year), Normal (4-12/year), Slow (1-4/year), Dead (<1/year)

---

## Part 2: DTOs (Data Transfer Objects)

### Directory: `D:\Majid\POS\pos-api\src\POSApi.Application\Common\DTOs\Analytics`

1. **RealTimeDashboardDto.cs**
   - Today's metrics: Sales, Orders, Customers, AvgOrderValue, Profit, ProfitMargin
   - Comparisons: YesterdaySales, WoWChange, MoMChange, YoYChange
   - Charts: HourlySales, TopProducts, TopCategories, TopCustomers, SalesByPaymentMethod
   - Inventory: LowStockItems, OutOfStockItems, TotalInventoryValue

2. **SalesForecastDto.cs**
   - Single forecast: Date, Product details, PredictedQuantity, PredictedRevenue, ConfidenceLevel
   - Summary: DailyForecasts list, TotalPredicted metrics, AverageConfidence, RecommendedAction

3. **InventoryTurnoverDto.cs**
   - Product-level: TurnoverRatio, DaysToSell, Classification, CurrentStock, EstimatedMonthsOfSupply
   - Summary: Overall metrics, Fast/Normal/Slow/Dead counts, ByCategory breakdown

4. **ABCAnalysisDto.cs**
   - Summary: Total products, counts by class, revenue by class, percentages
   - Details: Separate lists for Class A, B, C products with rank, contribution %, cumulative %
   - Recommendations: Strategy per classification

5. **SalesAnalyticsDto.cs**
   - Summary: TotalSales, Orders, AvgOrderValue, Profit, ProfitMargin
   - Trends: DailySales, WeeklySales, MonthlySales
   - Top performers: Products, Categories, Customers
   - Patterns: SalesByHour, SalesByDayOfWeek
   - Growth: GrowthRate, ComparisonPeriodSales, PercentageChange

6. **InventoryAnalyticsDto.cs**
   - Health metrics: TotalValue, TurnoverRate, HealthScore, CarryingCost, ObsolescenceRisk
   - Problem areas: LowStock, OutOfStock, Overstocked, DeadStock
   - Recommendations: Reorder lists, Overstocked items, Dead stock items

7. **CustomerAnalyticsDto.cs**
   - Metrics: Total/New/Active/Return customers, RetentionRate, ChurnRate, AvgLifetimeValue
   - Segmentation: Customer segments, RFM analysis
   - Top customers: By revenue and frequency

---

## Part 3: Services (Business Logic)

### Directory: `D:\Majid\POS\pos-api\src\POSApi.Application\Common\Services`

### 1. Sales Forecasting Service

**Files:**
- `ISalesForecastingService.cs`
- `SalesForecastingService.cs`

**Key Algorithms:**

#### Linear Regression Forecast
```
Uses least squares method:
- Calculate slope: Σ((x - x̄)(y - ȳ)) / Σ((x - x̄)²)
- Calculate intercept: ȳ - (slope × x̄)
- Prediction: y = slope × x + intercept
- Confidence interval: ±1.96 × standard_error (95% CI)
```

#### Moving Average Forecast
```
- Calculate average of last N days (window size)
- Use average as forecast for future periods
- Confidence interval: ±1.96 × standard_deviation
```

#### Exponential Smoothing
```
- Formula: St = α × Yt + (1 - α) × St-1
- α (alpha) = smoothing factor (0.3 default)
- Weights recent data more heavily
- Confidence interval: ±1.96 × MAE (Mean Absolute Error)
```

#### Auto-Selection Logic
```
Analyzes data characteristics:
- Strong trend + low variance → Linear Regression
- Low variance, stable → Moving Average
- Variable data → Exponential Smoothing

Trend calculation: (recent_half_avg - older_half_avg) / older_half_avg
Variance: Standard deviation / mean
```

**Methods:**
- `LinearRegressionForecast()`: Best for trending data
- `MovingAverageForecast()`: Best for stable data
- `ExponentialSmoothingForecast()`: Best for variable data
- `SelectBestMethodAndForecast()`: Auto-selects optimal method
- `CalculateConfidence()`: MAPE-based confidence scoring

---

### 2. ABC Analysis Service

**Files:**
- `IABCAnalysisService.cs`
- `ABCAnalysisService.cs`

**Pareto Principle Implementation:**

```
Classification Logic:
1. Sort products by revenue (descending)
2. Calculate cumulative revenue percentage
3. Classify:
   - Class A: 0-80% cumulative (typically ~20% of products)
   - Class B: 80-95% cumulative (typically ~30% of products)
   - Class C: 95-100% cumulative (typically ~50% of products)

Contribution % = (Product Revenue / Total Revenue) × 100
Cumulative % = Running total of contribution percentages
```

**Strategies by Class:**
- **Class A**: High priority - optimal stock, frequent monitoring, strong vendor relationships, accurate forecasting
- **Class B**: Medium priority - regular monitoring, balanced inventory, periodic demand review
- **Class C**: Low priority - minimize inventory, consider discontinuation, efficient ordering

**Methods:**
- `CalculateABCClassification()`: Full analysis for all products
- `ClassifyProduct()`: Classify based on cumulative percentage
- `GetRecommendedStrategy()`: Management strategy per class

---

### 3. Inventory Turnover Service

**Files:**
- `IInventoryTurnoverService.cs`
- `InventoryTurnoverService.cs`

**Turnover Calculation:**

```
Inventory Turnover Ratio = COGS / Average Inventory Value
COGS = Cost of Goods Sold (sum of cost × quantity sold)
Average Inventory = Current stock × unit cost

Annualized Turnover = Turnover Ratio × (365 / period_days)
Days to Sell = 365 / Annualized Turnover
Months of Supply = (Current Stock / Avg Daily Sales) / 30
```

**Classification Thresholds:**
- **Fast**: Turnover > 12 times/year (monthly turnover)
- **Normal**: 4-12 times/year (quarterly to monthly)
- **Slow**: 1-4 times/year (annually to quarterly)
- **Dead**: < 1 time/year (no meaningful movement)

**Reorder Calculation:**
```
Lead Time Demand = Avg Daily Sales × Lead Time Days (7 default)
Safety Stock = Avg Daily Sales × 14 days
Recommended Order Qty = Lead Time Demand + Safety Stock - Current Stock
Days Until Stockout = Current Stock / Avg Daily Sales

Priority:
- Critical: ≤3 days until stockout
- High: ≤7 days
- Medium: ≤14 days
- Low: >14 days
```

**Methods:**
- `CalculateTurnoverRatio()`: Full turnover analysis
- `IdentifySlowMovingStock()`: Filter slow/dead items
- `GenerateReorderRecommendations()`: Reorder suggestions with priority
- `ClassifyTurnoverRate()`: Classify by turnover speed

---

## Part 4: CQRS Handlers

### Queries Directory: `D:\Majid\POS\pos-api\src\POSApi.Application\Features\Analytics\Queries`

### 1. GetRealTimeDashboardQuery
**File:** `GetRealTimeDashboard\GetRealTimeDashboardQuery.cs`

**Features:**
- Today's sales, orders, customers, avg order value
- Profit and profit margin calculation
- Comparison metrics (YoY, MoM, WoW)
- Hourly sales breakdown
- Top 10 products, categories, customers
- Payment method breakdown
- Current inventory status (low stock, out of stock, total value)
- Caching recommended (1-5 minute TTL)

**SQL Optimization:**
- Uses date filtering with indexes
- Aggregates in-memory after fetch
- Excludes cancelled orders

---

### 2. GetSalesForecastQuery
**File:** `GetSalesForecast\GetSalesForecastQuery.cs`

**Parameters:**
- ProductId (required)
- StoreId (optional)
- ForecastDays (1-365, default 30)
- Method (LinearRegression/MovingAverage/ExponentialSmoothing/Auto)

**Features:**
- Uses historical 90-day data minimum
- Auto-selects best method if not specified
- Confidence intervals for each prediction
- Generates actionable recommendations
- Validates against reorder levels

**Validation:**
- Forecast days: 1-365
- Valid forecast methods only
- Product must exist

---

### 3. GetInventoryTurnoverQuery
**File:** `GetInventoryTurnover\GetInventoryTurnoverQuery.cs`

**Parameters:**
- StoreId (optional)
- CategoryId (optional)
- PeriodDays (default 365, max 730)

**Features:**
- Calculates turnover for all active products
- Groups by category
- Classifies by movement speed
- Estimates months of supply
- Generates recommendations per product

---

### 4. GetABCAnalysisQuery
**File:** `GetABCAnalysis\GetABCAnalysisQuery.cs`

**Parameters:**
- StoreId (optional)
- PeriodDays (default 365, max 730)

**Features:**
- Pareto analysis (80-15-5 rule)
- Ranks all products by revenue
- Calculates contribution percentages
- Provides management strategies
- Category-wise breakdown

---

### 5. GetSalesAnalyticsQuery
**File:** `GetSalesAnalytics\GetSalesAnalyticsQuery.cs`

**Parameters:**
- StartDate, EndDate (required)
- StoreId (optional)
- GroupBy (day/week/month)

**Features:**
- Comprehensive sales metrics
- Multiple trend views (daily, weekly, monthly)
- Top performers analysis
- Time-based patterns (hourly, day of week)
- Growth rate calculation vs comparison period
- Profit margin analysis

---

### Commands Directory: `D:\Majid\POS\pos-api\src\POSApi.Application\Features\Analytics\Commands`

### 1. GenerateSalesForecastCommand
**File:** `GenerateSalesForecast\GenerateSalesForecastCommand.cs`

**Features:**
- Batch forecast generation
- Auto-selects best method per product
- Saves to database for caching
- Deletes old forecasts
- Error handling per product (continues on failure)
- Background job recommended

**Parameters:**
- ProductIds (optional list, null = all products)
- StoreId (optional)
- ForecastDays (default 30)

---

### 2. CalculateABCClassificationCommand
**File:** `CalculateABCClassification\CalculateABCClassificationCommand.cs`

**Features:**
- Calculates ABC classification for all products
- Saves to database
- Deletes old classifications
- Can be scheduled (monthly recommended)
- Per-store classification support

**Parameters:**
- StoreId (optional)
- PeriodDays (default 365)

---

## Part 5: API Controller

### File: `D:\Majid\POS\pos-api\src\POSApi.Web.API\Controllers\AnalyticsController.cs`

### Endpoints:

#### Dashboard & Real-time
- `GET /api/analytics/dashboard/realtime?storeId={guid}`
  - Returns: RealTimeDashboardDto
  - Cache: 1-5 minutes recommended

#### Sales Analytics
- `GET /api/analytics/sales?startDate={date}&endDate={date}&storeId={guid}&groupBy={string}`
  - Returns: SalesAnalyticsDto
  - GroupBy: day, week, month

#### Sales Forecasting
- `GET /api/analytics/sales/forecast?productId={guid}&storeId={guid}&forecastDays={int}&method={string}`
  - Returns: SalesForecastSummaryDto
  - Method: LinearRegression, MovingAverage, ExponentialSmoothing, Auto

- `POST /api/analytics/sales/forecast/generate`
  - Body: GenerateSalesForecastCommand
  - Returns: bool
  - Use: Background job for bulk forecast generation

#### Inventory Analytics
- `GET /api/analytics/inventory/turnover?storeId={guid}&categoryId={guid}&periodDays={int}`
  - Returns: InventoryTurnoverSummaryDto

- `GET /api/analytics/inventory/abc-analysis?storeId={guid}&periodDays={int}`
  - Returns: ABCAnalysisDto

- `POST /api/analytics/inventory/abc-analysis/calculate`
  - Body: CalculateABCClassificationCommand
  - Returns: bool
  - Use: Scheduled recalculation (monthly)

- `GET /api/analytics/inventory/slow-moving?storeId={guid}`
  - Returns: List<InventoryTurnoverDto>

- `GET /api/analytics/inventory/reorder-recommendations?storeId={guid}`
  - Returns: List<ReorderRecommendationDto>

- `GET /api/analytics/inventory?storeId={guid}`
  - Returns: InventoryAnalyticsDto
  - Comprehensive inventory health report

---

## Database Integration Notes

### Entity Framework Configuration Required:

Add to your DbContext:

```csharp
public DbSet<SalesForecast> SalesForecasts { get; set; }
public DbSet<ProductABCClassification> ProductABCClassifications { get; set; }
public DbSet<InventoryTurnover> InventoryTurnovers { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // SalesForecast configuration
    modelBuilder.Entity<SalesForecast>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.ProductId, e.ForecastDate, e.StoreId });
        entity.Property(e => e.PredictedQuantity).HasPrecision(18, 2);
        entity.Property(e => e.PredictedRevenue).HasPrecision(18, 2);
        entity.Property(e => e.ConfidenceLevel).HasPrecision(5, 4);
    });

    // ProductABCClassification configuration
    modelBuilder.Entity<ProductABCClassification>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.ProductId, e.StoreId }).IsUnique();
        entity.Property(e => e.AnnualVolume).HasPrecision(18, 2);
        entity.Property(e => e.AnnualRevenue).HasPrecision(18, 2);
        entity.Property(e => e.ContributionPercentage).HasPrecision(5, 2);
        entity.Property(e => e.CumulativePercentage).HasPrecision(5, 2);
    });

    // InventoryTurnover configuration
    modelBuilder.Entity<InventoryTurnover>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.ProductId, e.StoreId, e.CalculationDate });
        entity.Property(e => e.TurnoverRatio).HasPrecision(18, 4);
        entity.Property(e => e.DaysToSell).HasPrecision(18, 2);
        entity.Property(e => e.AverageCOGS).HasPrecision(18, 2);
        entity.Property(e => e.AverageInventoryValue).HasPrecision(18, 2);
    });
}
```

### Create Migration:
```bash
dotnet ef migrations add AddAdvancedAnalytics
dotnet ef database update
```

---

## Dependency Injection Setup

Add to your Program.cs or Startup.cs:

```csharp
// Register services
services.AddScoped<ISalesForecastingService, SalesForecastingService>();
services.AddScoped<IABCAnalysisService, ABCAnalysisService>();
services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();

// FluentValidation will auto-register validators in the assembly
services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

---

## Performance Optimization Recommendations

### 1. Caching Strategy
```csharp
// Real-time dashboard - cache for 1-5 minutes
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "storeId" })]

// ABC Analysis - cache for 24 hours (recalculate daily)
[ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "storeId", "periodDays" })]

// Turnover analysis - cache for 6 hours
[ResponseCache(Duration = 21600, VaryByQueryKeys = new[] { "storeId", "categoryId" })]
```

### 2. Database Indexes
```sql
-- Essential indexes for performance
CREATE INDEX IX_Orders_OrderDate_Status ON Orders(OrderDate, Status);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
CREATE INDEX IX_Products_CategoryId_IsActive ON Products(CategoryId, IsActive);
CREATE INDEX IX_SalesForecasts_ProductId_ForecastDate ON SalesForecasts(ProductId, ForecastDate);
```

### 3. Background Jobs (Hangfire/Quartz)
```csharp
// Daily at 2 AM - Generate forecasts for all products
RecurringJob.AddOrUpdate(
    "generate-sales-forecasts",
    () => mediator.Send(new GenerateSalesForecastCommand { ForecastDays = 30 }),
    Cron.Daily(2));

// Monthly on 1st at 3 AM - Recalculate ABC classification
RecurringJob.AddOrUpdate(
    "calculate-abc-classification",
    () => mediator.Send(new CalculateABCClassificationCommand { PeriodDays = 365 }),
    Cron.Monthly(1, 3));
```

### 4. Pagination for Large Datasets
Consider adding pagination to:
- Top products/categories lists (limit to top 20-50)
- Inventory turnover items (paginate if >1000 products)
- Sales analytics daily data (limit date range or paginate)

---

## Testing Recommendations

### Unit Tests:
- Forecasting algorithms (linear regression, moving average, exponential smoothing)
- ABC classification logic (80-15-5 rule)
- Turnover calculation formulas
- Confidence interval calculations

### Integration Tests:
- Query handlers with test database
- Command handlers (forecast generation, ABC calculation)
- Service layer methods

### Performance Tests:
- Dashboard load time (<2 seconds target)
- Forecast generation for 1000 products (<5 minutes)
- ABC classification for 5000 products (<1 minute)

---

## Key Algorithms Summary

### 1. Linear Regression (Least Squares)
- **Use case**: Trending sales data
- **Formula**: y = mx + b
- **Confidence**: ±1.96 × SE (95% CI)

### 2. Moving Average
- **Use case**: Stable sales patterns
- **Formula**: Avg(last N values)
- **Confidence**: ±1.96 × σ

### 3. Exponential Smoothing
- **Use case**: Variable data with trends
- **Formula**: St = αYt + (1-α)St-1
- **Confidence**: ±1.96 × MAE

### 4. ABC Classification (Pareto)
- **Class A**: First 80% revenue (top ~20% products)
- **Class B**: Next 15% revenue (~30% products)
- **Class C**: Last 5% revenue (~50% products)

### 5. Inventory Turnover
- **Formula**: COGS / Avg Inventory Value
- **Classifications**: Fast (>12), Normal (4-12), Slow (1-4), Dead (<1)

---

## API Usage Examples

### 1. Get Real-time Dashboard
```http
GET /api/analytics/dashboard/realtime?storeId=123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {token}
```

### 2. Generate Sales Forecast
```http
GET /api/analytics/sales/forecast?productId=123e4567-e89b-12d3-a456-426614174000&forecastDays=30&method=Auto
Authorization: Bearer {token}
```

### 3. Get ABC Analysis
```http
GET /api/analytics/inventory/abc-analysis?periodDays=365
Authorization: Bearer {token}
```

### 4. Batch Generate Forecasts (Background Job)
```http
POST /api/analytics/sales/forecast/generate
Authorization: Bearer {token}
Content-Type: application/json

{
  "productIds": null,
  "storeId": null,
  "forecastDays": 30
}
```

### 5. Get Comprehensive Sales Analytics
```http
GET /api/analytics/sales?startDate=2025-01-01&endDate=2025-01-31&groupBy=day
Authorization: Bearer {token}
```

---

## Files Created Summary

### Domain Layer (1 file)
- `POSApi.Domain\Entities\AdvancedAnalytics.cs`

### Application Layer - DTOs (7 files)
- `POSApi.Application\Common\DTOs\Analytics\RealTimeDashboardDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\SalesForecastDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\InventoryTurnoverDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\ABCAnalysisDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\SalesAnalyticsDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\InventoryAnalyticsDto.cs`
- `POSApi.Application\Common\DTOs\Analytics\CustomerAnalyticsDto.cs`

### Application Layer - Services (6 files)
- `POSApi.Application\Common\Services\ISalesForecastingService.cs`
- `POSApi.Application\Common\Services\SalesForecastingService.cs`
- `POSApi.Application\Common\Services\IABCAnalysisService.cs`
- `POSApi.Application\Common\Services\ABCAnalysisService.cs`
- `POSApi.Application\Common\Services\IInventoryTurnoverService.cs`
- `POSApi.Application\Common\Services\InventoryTurnoverService.cs`

### Application Layer - Queries (5 files)
- `POSApi.Application\Features\Analytics\Queries\GetRealTimeDashboard\GetRealTimeDashboardQuery.cs`
- `POSApi.Application\Features\Analytics\Queries\GetSalesForecast\GetSalesForecastQuery.cs`
- `POSApi.Application\Features\Analytics\Queries\GetInventoryTurnover\GetInventoryTurnoverQuery.cs`
- `POSApi.Application\Features\Analytics\Queries\GetABCAnalysis\GetABCAnalysisQuery.cs`
- `POSApi.Application\Features\Analytics\Queries\GetSalesAnalytics\GetSalesAnalyticsQuery.cs`

### Application Layer - Commands (2 files)
- `POSApi.Application\Features\Analytics\Commands\GenerateSalesForecast\GenerateSalesForecastCommand.cs`
- `POSApi.Application\Features\Analytics\Commands\CalculateABCClassification\CalculateABCClassificationCommand.cs`

### Web API Layer (1 file)
- `POSApi.Web.API\Controllers\AnalyticsController.cs`

**Total: 22 files created**

---

## Next Steps

1. **Database Migration**: Run EF Core migrations to create new tables
2. **Dependency Injection**: Register services in Program.cs/Startup.cs
3. **Background Jobs**: Set up Hangfire/Quartz for scheduled tasks
4. **Caching**: Implement Redis/Memory cache for dashboard
5. **Testing**: Write unit and integration tests
6. **Documentation**: Update API documentation (Swagger)
7. **Monitoring**: Add logging and performance metrics
8. **UI Integration**: Connect frontend to new analytics endpoints

---

## Support & Maintenance

### Regular Tasks:
- **Daily**: Generate sales forecasts (background job)
- **Weekly**: Review slow-moving inventory
- **Monthly**: Recalculate ABC classification
- **Quarterly**: Review and adjust forecasting models

### Monitoring Metrics:
- Dashboard load time
- Forecast accuracy (MAPE)
- API response times
- Cache hit rates
- Background job success rates

---

*Implementation completed: 2025-11-18*
*Total development effort: ~22 files, ~4000+ lines of code*
