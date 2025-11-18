-- =====================================================
-- Advanced Features Seed Data
-- Created: 2025-11-18
-- Description: Seed data for Customer Tiers and Loyalty Program
-- =====================================================

BEGIN TRANSACTION;

DECLARE @BronzeTierId UNIQUEIDENTIFIER = NEWID();
DECLARE @SilverTierId UNIQUEIDENTIFIER = NEWID();
DECLARE @GoldTierId UNIQUEIDENTIFIER = NEWID();
DECLARE @PlatinumTierId UNIQUEIDENTIFIER = NEWID();
DECLARE @DefaultLoyaltyProgramId UNIQUEIDENTIFIER = NEWID();

-- =====================================================
-- PART 1: Seed Customer Tiers
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM [CustomerTier] WHERE [Name] = 'Bronze')
BEGIN
    INSERT INTO [CustomerTier] ([Id], [Name], [MinSpend], [MinPoints], [BenefitMultiplier], [DiscountPercentage], [Color], [SortOrder], [CreatedAt], [IsDeleted])
    VALUES (@BronzeTierId, 'Bronze', 0, 0, 1.0, 0, '#CD7F32', 1, GETUTCDATE(), 0);
    PRINT 'Bronze tier created';
END
ELSE
BEGIN
    SET @BronzeTierId = (SELECT TOP 1 [Id] FROM [CustomerTier] WHERE [Name] = 'Bronze');
    PRINT 'Bronze tier already exists';
END

IF NOT EXISTS (SELECT 1 FROM [CustomerTier] WHERE [Name] = 'Silver')
BEGIN
    INSERT INTO [CustomerTier] ([Id], [Name], [MinSpend], [MinPoints], [BenefitMultiplier], [DiscountPercentage], [Color], [SortOrder], [CreatedAt], [IsDeleted])
    VALUES (@SilverTierId, 'Silver', 1000, 500, 1.25, 5, '#C0C0C0', 2, GETUTCDATE(), 0);
    PRINT 'Silver tier created';
END
ELSE
BEGIN
    SET @SilverTierId = (SELECT TOP 1 [Id] FROM [CustomerTier] WHERE [Name] = 'Silver');
    PRINT 'Silver tier already exists';
END

IF NOT EXISTS (SELECT 1 FROM [CustomerTier] WHERE [Name] = 'Gold')
BEGIN
    INSERT INTO [CustomerTier] ([Id], [Name], [MinSpend], [MinPoints], [BenefitMultiplier], [DiscountPercentage], [Color], [SortOrder], [CreatedAt], [IsDeleted])
    VALUES (@GoldTierId, 'Gold', 5000, 2500, 1.5, 10, '#FFD700', 3, GETUTCDATE(), 0);
    PRINT 'Gold tier created';
END
ELSE
BEGIN
    SET @GoldTierId = (SELECT TOP 1 [Id] FROM [CustomerTier] WHERE [Name] = 'Gold');
    PRINT 'Gold tier already exists';
END

IF NOT EXISTS (SELECT 1 FROM [CustomerTier] WHERE [Name] = 'Platinum')
BEGIN
    INSERT INTO [CustomerTier] ([Id], [Name], [MinSpend], [MinPoints], [BenefitMultiplier], [DiscountPercentage], [Color], [SortOrder], [CreatedAt], [IsDeleted])
    VALUES (@PlatinumTierId, 'Platinum', 10000, 5000, 2.0, 15, '#E5E4E2', 4, GETUTCDATE(), 0);
    PRINT 'Platinum tier created';
END
ELSE
BEGIN
    SET @PlatinumTierId = (SELECT TOP 1 [Id] FROM [CustomerTier] WHERE [Name] = 'Platinum');
    PRINT 'Platinum tier already exists';
END

-- =====================================================
-- PART 2: Seed Default Loyalty Program
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM [LoyaltyProgram] WHERE [Name] = 'Default Loyalty Program')
BEGIN
    INSERT INTO [LoyaltyProgram] ([Id], [Name], [Description], [PointsPerDollar], [MinimumPurchaseAmount], [PointsExpiryDays], [IsActive], [CreatedAt], [IsDeleted])
    VALUES (
        @DefaultLoyaltyProgramId,
        'Default Loyalty Program',
        'Earn points on every purchase and redeem them for rewards. Points earned: 1 point per dollar spent. Points expire after 365 days.',
        1.0,  -- 1 point per dollar
        0,    -- No minimum purchase
        365,  -- Points expire after 1 year
        1,    -- Active
        GETUTCDATE(),
        0
    );
    PRINT 'Default Loyalty Program created and activated';
END
ELSE
BEGIN
    PRINT 'Default Loyalty Program already exists';
END

-- =====================================================
-- PART 3: Sample Rewards (Optional)
-- =====================================================

-- Sample Discount Reward
IF NOT EXISTS (SELECT 1 FROM [Reward] WHERE [Name] = '$5 Off Your Next Purchase')
BEGIN
    INSERT INTO [Reward] ([Id], [Name], [Description], [PointsCost], [RewardType], [Value], [ProductId], [ValidFrom], [ValidTo], [MaxRedemptionsPerCustomer], [TotalRedemptionsAllowed], [CurrentRedemptions], [IsActive], [CreatedAt], [IsDeleted])
    VALUES (
        NEWID(),
        '$5 Off Your Next Purchase',
        'Redeem this reward to get $5 off your next purchase of $25 or more.',
        500,  -- Costs 500 points
        'Discount',
        5.00,  -- $5 discount
        NULL,  -- Not tied to specific product
        GETUTCDATE(),
        DATEADD(YEAR, 1, GETUTCDATE()),  -- Valid for 1 year
        NULL,  -- No per-customer limit
        NULL,  -- No total limit
        0,
        1,  -- Active
        GETUTCDATE(),
        0
    );
    PRINT 'Sample $5 discount reward created';
END

-- Sample $10 Discount Reward
IF NOT EXISTS (SELECT 1 FROM [Reward] WHERE [Name] = '$10 Off Your Next Purchase')
BEGIN
    INSERT INTO [Reward] ([Id], [Name], [Description], [PointsCost], [RewardType], [Value], [ProductId], [ValidFrom], [ValidTo], [MaxRedemptionsPerCustomer], [TotalRedemptionsAllowed], [CurrentRedemptions], [IsActive], [CreatedAt], [IsDeleted])
    VALUES (
        NEWID(),
        '$10 Off Your Next Purchase',
        'Redeem this reward to get $10 off your next purchase of $50 or more.',
        1000,  -- Costs 1000 points
        'Discount',
        10.00,  -- $10 discount
        NULL,  -- Not tied to specific product
        GETUTCDATE(),
        DATEADD(YEAR, 1, GETUTCDATE()),  -- Valid for 1 year
        NULL,  -- No per-customer limit
        NULL,  -- No total limit
        0,
        1,  -- Active
        GETUTCDATE(),
        0
    );
    PRINT 'Sample $10 discount reward created';
END

-- Sample Cashback Reward
IF NOT EXISTS (SELECT 1 FROM [Reward] WHERE [Name] = '$25 Cashback')
BEGIN
    INSERT INTO [Reward] ([Id], [Name], [Description], [PointsCost], [RewardType], [Value], [ProductId], [ValidFrom], [ValidTo], [MaxRedemptionsPerCustomer], [TotalRedemptionsAllowed], [CurrentRedemptions], [IsActive], [CreatedAt], [IsDeleted])
    VALUES (
        NEWID(),
        '$25 Cashback',
        'Redeem this reward for $25 cashback to your account.',
        2500,  -- Costs 2500 points
        'Cashback',
        25.00,  -- $25 cashback
        NULL,  -- Not tied to specific product
        GETUTCDATE(),
        DATEADD(YEAR, 1, GETUTCDATE()),  -- Valid for 1 year
        4,  -- Max 4 per customer per year
        NULL,  -- No total limit
        0,
        1,  -- Active
        GETUTCDATE(),
        0
    );
    PRINT 'Sample $25 cashback reward created';
END

COMMIT TRANSACTION;

PRINT '';
PRINT '========================================';
PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT 'Customer Tiers Created:';
PRINT '  - Bronze (0+ points, 0% discount, 1x multiplier)';
PRINT '  - Silver (500+ points, 5% discount, 1.25x multiplier)';
PRINT '  - Gold (2500+ points, 10% discount, 1.5x multiplier)';
PRINT '  - Platinum (5000+ points, 15% discount, 2x multiplier)';
PRINT '';
PRINT 'Default Loyalty Program:';
PRINT '  - 1 point per dollar spent';
PRINT '  - Points expire after 365 days';
PRINT '  - Status: Active';
PRINT '';
PRINT 'Sample Rewards:';
PRINT '  - $5 Off (500 points)';
PRINT '  - $10 Off (1000 points)';
PRINT '  - $25 Cashback (2500 points, max 4/customer)';
PRINT '========================================';
GO
