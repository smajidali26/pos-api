-- =====================================================
-- Add Advanced Features - ROLLBACK Script
-- Created: 2025-11-18
-- Description: Safely removes all Advanced Features tables
-- WARNING: This will DELETE ALL DATA in these tables!
-- =====================================================

PRINT '========================================';
PRINT 'Starting Rollback of Advanced Features';
PRINT 'WARNING: This will delete all data!';
PRINT '========================================';
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY
    -- =====================================================
    -- PART 1: Drop Loyalty Program Tables (in reverse order of dependencies)
    -- =====================================================

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RewardRedemption]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[RewardRedemption];
        PRINT 'Dropped table: RewardRedemption';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reward]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[Reward];
        PRINT 'Dropped table: Reward';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyTransaction]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[LoyaltyTransaction];
        PRINT 'Dropped table: LoyaltyTransaction';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLoyalty]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[CustomerLoyalty];
        PRINT 'Dropped table: CustomerLoyalty';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerTier]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[CustomerTier];
        PRINT 'Dropped table: CustomerTier';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyProgram]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[LoyaltyProgram];
        PRINT 'Dropped table: LoyaltyProgram';
    END

    -- =====================================================
    -- PART 2: Drop Analytics Tables
    -- =====================================================

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryTurnover]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[InventoryTurnover];
        PRINT 'Dropped table: InventoryTurnover';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductABCClassification]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[ProductABCClassification];
        PRINT 'Dropped table: ProductABCClassification';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesForecast]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[SalesForecast];
        PRINT 'Dropped table: SalesForecast';
    END

    -- =====================================================
    -- PART 3: Drop Multi-Store Tables (in reverse order of dependencies)
    -- =====================================================

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InterStoreTransferItem]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[InterStoreTransferItem];
        PRINT 'Dropped table: InterStoreTransferItem';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InterStoreTransfer]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[InterStoreTransfer];
        PRINT 'Dropped table: InterStoreTransfer';
    END

    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreInventory]') AND type in (N'U'))
    BEGIN
        DROP TABLE [dbo].[StoreInventory];
        PRINT 'Dropped table: StoreInventory';
    END

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '========================================';
    PRINT 'Rollback completed successfully!';
    PRINT '';
    PRINT 'Removed tables:';
    PRINT '  Loyalty Program: 6 tables';
    PRINT '  Analytics: 3 tables';
    PRINT '  Multi-Store: 3 tables';
    PRINT '';
    PRINT 'Total: 12 tables removed';
    PRINT '========================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '========================================';
    PRINT 'ERROR: Rollback failed!';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
    PRINT '========================================';

    -- Re-throw the error
    THROW;
END CATCH
GO
