-- =====================================================
-- Add Advanced Features Migration
-- Created: 2025-11-18
-- Description: Adds Multi-Store, Analytics, and Loyalty Program tables
-- =====================================================

BEGIN TRANSACTION;

-- =====================================================
-- PART 1: Multi-Store Management Tables
-- =====================================================

-- Table: StoreInventory
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreInventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StoreInventory] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [StoreId] UNIQUEIDENTIFIER NOT NULL,
        [ProductId] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] INT NOT NULL DEFAULT 0,
        [MinStockLevel] INT NOT NULL DEFAULT 0,
        [MaxStockLevel] INT NOT NULL DEFAULT 0,
        [ReorderPoint] INT NOT NULL DEFAULT 0,
        [LastRestockedAt] DATETIME2(7) NULL,
        [LastSoldAt] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_StoreInventory_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StoreInventory_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_StoreInventory_Quantity] CHECK ([Quantity] >= 0),
        CONSTRAINT [CK_StoreInventory_MinStock] CHECK ([MinStockLevel] >= 0),
        CONSTRAINT [CK_StoreInventory_MaxStock] CHECK ([MaxStockLevel] >= 0),
        CONSTRAINT [CK_StoreInventory_ReorderPoint] CHECK ([ReorderPoint] >= 0)
    );

    CREATE UNIQUE INDEX [IX_StoreInventory_Store_Product] ON [StoreInventory]([StoreId], [ProductId]);
    CREATE INDEX [IX_StoreInventory_LastRestockedAt] ON [StoreInventory]([LastRestockedAt]);
    CREATE INDEX [IX_StoreInventory_LastSoldAt] ON [StoreInventory]([LastSoldAt]);
    CREATE INDEX [IX_StoreInventory_Quantity_MinStockLevel] ON [StoreInventory]([Quantity], [MinStockLevel]);
END
GO

-- Table: InterStoreTransfer
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InterStoreTransfer]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InterStoreTransfer] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [TransferNumber] NVARCHAR(50) NOT NULL,
        [FromStoreId] UNIQUEIDENTIFIER NOT NULL,
        [ToStoreId] UNIQUEIDENTIFIER NOT NULL,
        [RequestedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [ApprovedByUserId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
        [RequestDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [ApprovalDate] DATETIME2(7) NULL,
        [ShipDate] DATETIME2(7) NULL,
        [ReceiveDate] DATETIME2(7) NULL,
        [Notes] NVARCHAR(2000) NULL,
        [RejectionReason] NVARCHAR(1000) NULL,
        [CancellationReason] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_InterStoreTransfer_FromStore] FOREIGN KEY ([FromStoreId]) REFERENCES [Store]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InterStoreTransfer_ToStore] FOREIGN KEY ([ToStoreId]) REFERENCES [Store]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InterStoreTransfer_RequestedBy] FOREIGN KEY ([RequestedByUserId]) REFERENCES [User]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InterStoreTransfer_ApprovedBy] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [User]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_InterStoreTransfer_DifferentStores] CHECK ([FromStoreId] <> [ToStoreId])
    );

    CREATE UNIQUE INDEX [IX_InterStoreTransfer_TransferNumber] ON [InterStoreTransfer]([TransferNumber]);
    CREATE INDEX [IX_InterStoreTransfer_Status] ON [InterStoreTransfer]([Status]);
    CREATE INDEX [IX_InterStoreTransfer_RequestDate] ON [InterStoreTransfer]([RequestDate]);
    CREATE INDEX [IX_InterStoreTransfer_FromStore_Status] ON [InterStoreTransfer]([FromStoreId], [Status]);
    CREATE INDEX [IX_InterStoreTransfer_ToStore_Status] ON [InterStoreTransfer]([ToStoreId], [Status]);
END
GO

-- Table: InterStoreTransferItem
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InterStoreTransferItem]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InterStoreTransferItem] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [TransferId] UNIQUEIDENTIFIER NOT NULL,
        [ProductId] UNIQUEIDENTIFIER NOT NULL,
        [RequestedQuantity] INT NOT NULL,
        [ApprovedQuantity] INT NOT NULL DEFAULT 0,
        [ShippedQuantity] INT NOT NULL DEFAULT 0,
        [ReceivedQuantity] INT NOT NULL DEFAULT 0,
        [UnitCost] DECIMAL(18, 2) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_InterStoreTransferItem_Transfer] FOREIGN KEY ([TransferId]) REFERENCES [InterStoreTransfer]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InterStoreTransferItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_InterStoreTransferItem_RequestedQuantity] CHECK ([RequestedQuantity] > 0),
        CONSTRAINT [CK_InterStoreTransferItem_ApprovedQuantity] CHECK ([ApprovedQuantity] >= 0),
        CONSTRAINT [CK_InterStoreTransferItem_ShippedQuantity] CHECK ([ShippedQuantity] >= 0),
        CONSTRAINT [CK_InterStoreTransferItem_ReceivedQuantity] CHECK ([ReceivedQuantity] >= 0)
    );

    CREATE INDEX [IX_InterStoreTransferItem_TransferId] ON [InterStoreTransferItem]([TransferId]);
    CREATE INDEX [IX_InterStoreTransferItem_ProductId] ON [InterStoreTransferItem]([ProductId]);
END
GO

-- =====================================================
-- PART 2: Analytics Tables
-- =====================================================

-- Table: SalesForecast
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesForecast]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SalesForecast] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ForecastDate] DATETIME2(7) NOT NULL,
        [ProductId] UNIQUEIDENTIFIER NOT NULL,
        [StoreId] UNIQUEIDENTIFIER NULL,
        [PredictedQuantity] DECIMAL(18, 2) NOT NULL,
        [PredictedRevenue] DECIMAL(18, 2) NOT NULL,
        [ConfidenceLevel] DECIMAL(5, 4) NOT NULL,
        [ForecastMethod] NVARCHAR(50) NOT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_SalesForecast_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalesForecast_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_SalesForecast_ConfidenceLevel] CHECK ([ConfidenceLevel] >= 0 AND [ConfidenceLevel] <= 1)
    );

    CREATE INDEX [IX_SalesForecast_Product_Store_Date] ON [SalesForecast]([ProductId], [StoreId], [ForecastDate]);
    CREATE INDEX [IX_SalesForecast_CreatedAt] ON [SalesForecast]([CreatedAt]);
    CREATE INDEX [IX_SalesForecast_ForecastMethod] ON [SalesForecast]([ForecastMethod]);
END
GO

-- Table: ProductABCClassification
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductABCClassification]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductABCClassification] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ProductId] UNIQUEIDENTIFIER NOT NULL,
        [StoreId] UNIQUEIDENTIFIER NULL,
        [Classification] NVARCHAR(10) NOT NULL,
        [AnnualVolume] DECIMAL(18, 2) NOT NULL,
        [AnnualRevenue] DECIMAL(18, 2) NOT NULL,
        [ContributionPercentage] DECIMAL(5, 4) NOT NULL,
        [LastCalculatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Rank] INT NOT NULL,
        [CumulativePercentage] DECIMAL(5, 4) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_ProductABCClassification_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductABCClassification_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_ProductABCClassification_Classification] CHECK ([Classification] IN ('A', 'B', 'C'))
    );

    CREATE UNIQUE INDEX [IX_ProductABCClassification_Product_Store] ON [ProductABCClassification]([ProductId], [StoreId]);
    CREATE INDEX [IX_ProductABCClassification_Classification] ON [ProductABCClassification]([Classification]);
    CREATE INDEX [IX_ProductABCClassification_LastCalculatedAt] ON [ProductABCClassification]([LastCalculatedAt]);
    CREATE INDEX [IX_ProductABCClassification_Rank] ON [ProductABCClassification]([Rank]);
END
GO

-- Table: InventoryTurnover
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryTurnover]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InventoryTurnover] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ProductId] UNIQUEIDENTIFIER NOT NULL,
        [StoreId] UNIQUEIDENTIFIER NULL,
        [CalculationDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [PeriodDays] INT NOT NULL,
        [TurnoverRatio] DECIMAL(18, 4) NOT NULL,
        [DaysToSell] DECIMAL(18, 2) NOT NULL,
        [AverageCOGS] DECIMAL(18, 2) NOT NULL,
        [AverageInventoryValue] DECIMAL(18, 2) NOT NULL,
        [Classification] NVARCHAR(50) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_InventoryTurnover_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryTurnover_Store] FOREIGN KEY ([StoreId]) REFERENCES [Store]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_InventoryTurnover_Classification] CHECK ([Classification] IN ('Fast', 'Normal', 'Slow', 'Dead'))
    );

    CREATE INDEX [IX_InventoryTurnover_Product_Store] ON [InventoryTurnover]([ProductId], [StoreId]);
    CREATE INDEX [IX_InventoryTurnover_Classification] ON [InventoryTurnover]([Classification]);
    CREATE INDEX [IX_InventoryTurnover_CalculationDate] ON [InventoryTurnover]([CalculationDate]);
END
GO

-- =====================================================
-- PART 3: Loyalty Program Tables
-- =====================================================

-- Table: LoyaltyProgram
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyProgram]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyProgram] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(2000) NULL,
        [PointsPerDollar] DECIMAL(5, 2) NOT NULL,
        [MinimumPurchaseAmount] DECIMAL(18, 2) NOT NULL,
        [PointsExpiryDays] INT NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [CK_LoyaltyProgram_PointsPerDollar] CHECK ([PointsPerDollar] >= 0),
        CONSTRAINT [CK_LoyaltyProgram_MinimumPurchase] CHECK ([MinimumPurchaseAmount] >= 0),
        CONSTRAINT [CK_LoyaltyProgram_ExpiryDays] CHECK ([PointsExpiryDays] >= 0)
    );

    CREATE INDEX [IX_LoyaltyProgram_IsActive] ON [LoyaltyProgram]([IsActive]);
END
GO

-- Table: CustomerTier
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerTier]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CustomerTier] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(100) NOT NULL,
        [MinSpend] DECIMAL(18, 2) NOT NULL,
        [MinPoints] INT NOT NULL,
        [BenefitMultiplier] DECIMAL(5, 2) NOT NULL,
        [DiscountPercentage] DECIMAL(5, 2) NOT NULL,
        [Color] NVARCHAR(50) NULL,
        [SortOrder] INT NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [CK_CustomerTier_MinSpend] CHECK ([MinSpend] >= 0),
        CONSTRAINT [CK_CustomerTier_MinPoints] CHECK ([MinPoints] >= 0),
        CONSTRAINT [CK_CustomerTier_BenefitMultiplier] CHECK ([BenefitMultiplier] >= 0),
        CONSTRAINT [CK_CustomerTier_DiscountPercentage] CHECK ([DiscountPercentage] >= 0 AND [DiscountPercentage] <= 100)
    );

    CREATE UNIQUE INDEX [IX_CustomerTier_Name] ON [CustomerTier]([Name]);
    CREATE INDEX [IX_CustomerTier_MinPoints] ON [CustomerTier]([MinPoints]);
    CREATE INDEX [IX_CustomerTier_SortOrder] ON [CustomerTier]([SortOrder]);
END
GO

-- Table: CustomerLoyalty
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLoyalty]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CustomerLoyalty] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [CurrentPoints] INT NOT NULL DEFAULT 0,
        [LifetimePoints] INT NOT NULL DEFAULT 0,
        [LifetimeSpend] DECIMAL(18, 2) NOT NULL DEFAULT 0,
        [CurrentTierId] UNIQUEIDENTIFIER NULL,
        [JoinDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LastActivityDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [LastPointsExpiryCheckDate] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_CustomerLoyalty_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CustomerLoyalty_CustomerTier] FOREIGN KEY ([CurrentTierId]) REFERENCES [CustomerTier]([Id]) ON DELETE SET NULL,
        CONSTRAINT [CK_CustomerLoyalty_CurrentPoints] CHECK ([CurrentPoints] >= 0),
        CONSTRAINT [CK_CustomerLoyalty_LifetimePoints] CHECK ([LifetimePoints] >= 0),
        CONSTRAINT [CK_CustomerLoyalty_LifetimeSpend] CHECK ([LifetimeSpend] >= 0)
    );

    CREATE UNIQUE INDEX [IX_CustomerLoyalty_CustomerId] ON [CustomerLoyalty]([CustomerId]);
    CREATE INDEX [IX_CustomerLoyalty_CurrentTierId] ON [CustomerLoyalty]([CurrentTierId]);
    CREATE INDEX [IX_CustomerLoyalty_JoinDate] ON [CustomerLoyalty]([JoinDate]);
    CREATE INDEX [IX_CustomerLoyalty_LastActivityDate] ON [CustomerLoyalty]([LastActivityDate]);
END
GO

-- Table: LoyaltyTransaction
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyTransaction]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyTransaction] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [CustomerLoyaltyId] UNIQUEIDENTIFIER NOT NULL,
        [OrderId] UNIQUEIDENTIFIER NULL,
        [PointsEarned] INT NOT NULL DEFAULT 0,
        [PointsRedeemed] INT NOT NULL DEFAULT 0,
        [BalanceBefore] INT NOT NULL,
        [BalanceAfter] INT NOT NULL,
        [TransactionDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Description] NVARCHAR(500) NOT NULL,
        [TransactionType] NVARCHAR(50) NOT NULL,
        [ExpiryDate] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_LoyaltyTransaction_CustomerLoyalty] FOREIGN KEY ([CustomerLoyaltyId]) REFERENCES [CustomerLoyalty]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LoyaltyTransaction_Order] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id]) ON DELETE SET NULL,
        CONSTRAINT [CK_LoyaltyTransaction_PointsEarned] CHECK ([PointsEarned] >= 0),
        CONSTRAINT [CK_LoyaltyTransaction_PointsRedeemed] CHECK ([PointsRedeemed] >= 0),
        CONSTRAINT [CK_LoyaltyTransaction_TransactionType] CHECK ([TransactionType] IN ('Earned', 'Redeemed', 'Expired', 'Adjusted', 'Bonus'))
    );

    CREATE INDEX [IX_LoyaltyTransaction_CustomerLoyaltyId] ON [LoyaltyTransaction]([CustomerLoyaltyId]);
    CREATE INDEX [IX_LoyaltyTransaction_TransactionDate] ON [LoyaltyTransaction]([TransactionDate]);
    CREATE INDEX [IX_LoyaltyTransaction_TransactionType] ON [LoyaltyTransaction]([TransactionType]);
    CREATE INDEX [IX_LoyaltyTransaction_ExpiryDate] ON [LoyaltyTransaction]([ExpiryDate]);
    CREATE INDEX [IX_LoyaltyTransaction_TransactionType_ExpiryDate] ON [LoyaltyTransaction]([TransactionType], [ExpiryDate]);
END
GO

-- Table: Reward
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reward]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Reward] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(2000) NULL,
        [PointsCost] INT NOT NULL,
        [RewardType] NVARCHAR(50) NOT NULL,
        [Value] DECIMAL(18, 2) NOT NULL,
        [ProductId] UNIQUEIDENTIFIER NULL,
        [ValidFrom] DATETIME2(7) NOT NULL,
        [ValidTo] DATETIME2(7) NOT NULL,
        [MaxRedemptionsPerCustomer] INT NULL,
        [TotalRedemptionsAllowed] INT NULL,
        [CurrentRedemptions] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Reward_Product] FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]) ON DELETE SET NULL,
        CONSTRAINT [CK_Reward_PointsCost] CHECK ([PointsCost] > 0),
        CONSTRAINT [CK_Reward_Value] CHECK ([Value] >= 0),
        CONSTRAINT [CK_Reward_RewardType] CHECK ([RewardType] IN ('Discount', 'FreeProduct', 'Cashback', 'ServiceVoucher')),
        CONSTRAINT [CK_Reward_ValidPeriod] CHECK ([ValidTo] >= [ValidFrom]),
        CONSTRAINT [CK_Reward_CurrentRedemptions] CHECK ([CurrentRedemptions] >= 0)
    );

    CREATE INDEX [IX_Reward_IsActive] ON [Reward]([IsActive]);
    CREATE INDEX [IX_Reward_ValidFrom_ValidTo] ON [Reward]([ValidFrom], [ValidTo]);
    CREATE INDEX [IX_Reward_RewardType] ON [Reward]([RewardType]);
    CREATE INDEX [IX_Reward_PointsCost] ON [Reward]([PointsCost]);
END
GO

-- Table: RewardRedemption
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RewardRedemption]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RewardRedemption] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [RewardId] UNIQUEIDENTIFIER NOT NULL,
        [CustomerLoyaltyId] UNIQUEIDENTIFIER NOT NULL,
        [OrderId] UNIQUEIDENTIFIER NULL,
        [PointsUsed] INT NOT NULL,
        [RedeemedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UsedAt] DATETIME2(7) NULL,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [ExpiryDate] DATETIME2(7) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_RewardRedemption_Reward] FOREIGN KEY ([RewardId]) REFERENCES [Reward]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RewardRedemption_CustomerLoyalty] FOREIGN KEY ([CustomerLoyaltyId]) REFERENCES [CustomerLoyalty]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RewardRedemption_Order] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id]) ON DELETE SET NULL,
        CONSTRAINT [CK_RewardRedemption_PointsUsed] CHECK ([PointsUsed] > 0)
    );

    CREATE INDEX [IX_RewardRedemption_RewardId] ON [RewardRedemption]([RewardId]);
    CREATE INDEX [IX_RewardRedemption_CustomerLoyaltyId] ON [RewardRedemption]([CustomerLoyaltyId]);
    CREATE INDEX [IX_RewardRedemption_RedeemedAt] ON [RewardRedemption]([RedeemedAt]);
    CREATE INDEX [IX_RewardRedemption_IsUsed] ON [RewardRedemption]([IsUsed]);
    CREATE INDEX [IX_RewardRedemption_ExpiryDate] ON [RewardRedemption]([ExpiryDate]);
    CREATE INDEX [IX_RewardRedemption_CustomerLoyalty_IsUsed] ON [RewardRedemption]([CustomerLoyaltyId], [IsUsed]);
END
GO

COMMIT TRANSACTION;

PRINT 'Advanced Features migration completed successfully!';
GO
