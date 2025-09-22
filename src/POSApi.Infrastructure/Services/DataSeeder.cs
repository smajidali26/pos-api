using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using BCrypt.Net;

namespace POSApi.Infrastructure.Services;

public class DataSeeder
{
    private readonly PosDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(PosDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Database creation is handled in Program.cs
            // This method only handles data seeding

            // Seed default users first (required for other entities)
            await SeedUsersAsync();

            // Seed unit types and units of measure
            await SeedUnitsOfMeasureAsync();

            // Seed categories
            await SeedCategoriesAsync();

            // Seed vendors
            await SeedVendorsAsync();

            // Seed products with UOM
            await SeedProductsAsync();

            // Seed customers
            await SeedCustomersAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping user seeding");
            return;
        }

        var users = new[]
        {
            new User("admin", "System", "Administrator", "admin@pos.com", 
                BCrypt.Net.BCrypt.HashPassword("Admin@123"), UserRole.Owner),
            new User("manager", "Store", "Manager", "manager@pos.com", 
                BCrypt.Net.BCrypt.HashPassword("Manager@123"), UserRole.Manager),
            new User("cashier1", "John", "Doe", "john.doe@pos.com", 
                BCrypt.Net.BCrypt.HashPassword("Cashier@123"), UserRole.Cashier),
            new User("cashier2", "Jane", "Smith", "jane.smith@pos.com", 
                BCrypt.Net.BCrypt.HashPassword("Cashier@123"), UserRole.Cashier)
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} default users", users.Length);
    }

    private async Task SeedUnitsOfMeasureAsync()
    {
        if (await _context.UnitTypes.AnyAsync())
        {
            _logger.LogInformation("Unit types already exist, skipping UOM seeding");
            return;
        }

        // Seed Unit Types
        var unitTypes = new[]
        {
            new UnitType("Count", "Discrete counting units", 1),
            new UnitType("Weight", "Weight and mass measurements", 2),
            new UnitType("Volume", "Volume and liquid measurements", 3),
            new UnitType("Length", "Length and distance measurements", 4)
        };

        await _context.UnitTypes.AddRangeAsync(unitTypes);
        await _context.SaveChangesAsync();

        // Seed Units of Measure
        var countType = unitTypes[0];
        var weightType = unitTypes[1];
        var volumeType = unitTypes[2];
        var lengthType = unitTypes[3];

        var unitsOfMeasure = new[]
        {
            // Count units
            new UnitOfMeasure("PCS", "Piece", "pcs", countType.Id, 1.0m, null, "pcs", 1),
            new UnitOfMeasure("EACH", "Each", "ea", countType.Id, 1.0m, null, "ea", 2),
            new UnitOfMeasure("DOZEN", "Dozen", "doz", countType.Id, 12.0m, null, "dz", 3),
            new UnitOfMeasure("PACK", "Pack", "pack", countType.Id, 1.0m, null, "pk", 4),

            // Weight units
            new UnitOfMeasure("KG", "Kilogram", "kg", weightType.Id, 1.0m, null, "kg", 1),
            new UnitOfMeasure("G", "Gram", "g", weightType.Id, 0.001m, null, "g", 2),
            new UnitOfMeasure("LB", "Pound", "lb", weightType.Id, 0.453592m, null, "lb", 3),
            new UnitOfMeasure("OZ", "Ounce", "oz", weightType.Id, 0.0283495m, null, "oz", 4),

            // Volume units
            new UnitOfMeasure("L", "Liter", "L", volumeType.Id, 1.0m, null, "L", 1),
            new UnitOfMeasure("ML", "Milliliter", "mL", volumeType.Id, 0.001m, null, "mL", 2),
            new UnitOfMeasure("GAL", "Gallon", "gal", volumeType.Id, 3.78541m, null, "gal", 3),
            new UnitOfMeasure("FLOZ", "Fluid Ounce", "fl oz", volumeType.Id, 0.0295735m, null, "fl oz", 4),

            // Length units
            new UnitOfMeasure("M", "Meter", "m", lengthType.Id, 1.0m, null, "m", 1),
            new UnitOfMeasure("CM", "Centimeter", "cm", lengthType.Id, 0.01m, null, "cm", 2),
            new UnitOfMeasure("IN", "Inch", "in", lengthType.Id, 0.0254m, null, "in", 3),
            new UnitOfMeasure("FT", "Foot", "ft", lengthType.Id, 0.3048m, null, "ft", 4)
        };

        await _context.UnitsOfMeasure.AddRangeAsync(unitsOfMeasure);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {TypeCount} unit types and {UnitCount} units of measure", 
            unitTypes.Length, unitsOfMeasure.Length);
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist, skipping category seeding");
            return;
        }

        var categories = new[]
        {
            new Category("Electronics", "Electronic devices and accessories"),
            new Category("Clothing", "Apparel and fashion items"),
            new Category("Food & Beverages", "Food items and drinks"),
            new Category("Books", "Books and educational materials"),
            new Category("Home & Garden", "Home improvement and gardening supplies"),
            new Category("Sports", "Sports equipment and accessories"),
            new Category("Health & Beauty", "Health and beauty products"),
            new Category("Toys", "Toys and games")
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} categories", categories.Length);
    }

    private async Task SeedVendorsAsync()
    {
        if (await _context.Vendors.AnyAsync())
        {
            _logger.LogInformation("Vendors already exist, skipping vendor seeding");
            return;
        }

        var vendors = new[]
        {
            new Vendor("TechCorp", "TechCorp Industries", "John Technology", "john@techcorp.com", 
                "+1-555-0101", "123 Tech Street", "San Francisco", "CA", "94105", "USA", VendorType.Manufacturer),
            
            new Vendor("FashionPlus", "Fashion Plus Ltd", "Sarah Fashion", "sarah@fashionplus.com", 
                "+1-555-0102", "456 Fashion Ave", "New York", "NY", "10001", "USA", VendorType.Distributor),
            
            new Vendor("FoodSupply", "Food Supply Co", "Mike Supplier", "mike@foodsupply.com", 
                "+1-555-0103", "789 Food Blvd", "Chicago", "IL", "60601", "USA", VendorType.Wholesaler)
        };

        // Update additional vendor properties
        vendors[0].UpdateCreditLimit(50000m);
        vendors[0].UpdatePaymentTerms(PaymentTerms.Net30);
        vendors[0].UpdateTaxId("12-3456789");
        vendors[0].UpdateWebsite("https://techcorp.com");
        vendors[0].UpdateNotes("Primary electronics supplier");

        vendors[1].UpdateCreditLimit(30000m);
        vendors[1].UpdatePaymentTerms(PaymentTerms.Net15);
        vendors[1].UpdateTaxId("98-7654321");
        vendors[1].UpdateWebsite("https://fashionplus.com");
        vendors[1].UpdateNotes("Clothing and apparel supplier");

        vendors[2].UpdateCreditLimit(20000m);
        vendors[2].UpdatePaymentTerms(PaymentTerms.Net15);
        vendors[2].UpdateTaxId("45-6789123");
        vendors[2].UpdateWebsite("https://foodsupply.com");
        vendors[2].UpdateNotes("Food and beverage wholesaler");

        await _context.Vendors.AddRangeAsync(vendors);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} vendors", vendors.Length);
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist, skipping product seeding");
            return;
        }

        var categories = await _context.Categories.ToListAsync();
        var vendors = await _context.Vendors.ToListAsync();
        var pcsUnit = await _context.UnitsOfMeasure.FirstAsync(u => u.Code == "PCS");

        var electronicsCategory = categories.First(c => c.Name == "Electronics");
        var clothingCategory = categories.First(c => c.Name == "Clothing");
        var foodCategory = categories.First(c => c.Name == "Food & Beverages");
        var techVendor = vendors.First(v => v.Name == "TechCorp");
        var fashionVendor = vendors.First(v => v.Name == "FashionPlus");
        var foodVendor = vendors.First(v => v.Name == "FoodSupply");

        var products = new[]
        {
            new Product("Wireless Mouse", "Ergonomic wireless mouse with USB receiver", 
                "WM001", "123456789012", 29.99m, 15.00m, 100, 10, electronicsCategory.Id, 15, 50),
            
            new Product("Bluetooth Keyboard", "Wireless Bluetooth keyboard", 
                "KB001", "123456789013", 79.99m, 40.00m, 50, 5, electronicsCategory.Id, 10, 25),
            
            new Product("Cotton T-Shirt", "100% cotton casual t-shirt", 
                "TS001", "123456789014", 19.99m, 8.00m, 200, 20, clothingCategory.Id, 30, 100),
            
            new Product("Jeans", "Classic denim jeans", 
                "JN001", "123456789015", 49.99m, 25.00m, 75, 10, clothingCategory.Id, 15, 50),
            
            new Product("Organic Coffee", "Premium organic coffee beans", 
                "CF001", "123456789016", 12.99m, 6.00m, 150, 15, foodCategory.Id, 25, 100),
            
            new Product("Green Tea", "Organic green tea leaves", 
                "GT001", "123456789017", 8.99m, 4.00m, 100, 10, foodCategory.Id, 20, 75)
        };

        // Set vendors for products
        products[0].SetPrimaryVendor(techVendor.Id, "WM-001");
        products[1].SetPrimaryVendor(techVendor.Id, "KB-001");
        products[2].SetPrimaryVendor(fashionVendor.Id, "TS-001");
        products[3].SetPrimaryVendor(fashionVendor.Id, "JN-001");
        products[4].SetPrimaryVendor(foodVendor.Id, "CF-001");
        products[5].SetPrimaryVendor(foodVendor.Id, "GT-001");

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Create product units for each product
        var productUnits = new List<ProductUnit>();
        foreach (var product in products)
        {
            var productUnit = new ProductUnit(product.Id, pcsUnit.Id, 1.0m);
            productUnits.Add(productUnit);
        }

        await _context.ProductUnits.AddRangeAsync(productUnits);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} products with units of measure", products.Length);
    }

    private async Task SeedCustomersAsync()
    {
        if (await _context.Customers.AnyAsync())
        {
            _logger.LogInformation("Customers already exist, skipping customer seeding");
            return;
        }

        var customers = new[]
        {
            new Customer("John", "Smith", "john.smith@email.com", "+1-555-1001", 
                "123 Main St", "Springfield", "IL", "62701", new DateTime(1985, 5, 15)),
            
            new Customer("Emily", "Johnson", "emily.johnson@email.com", "+1-555-1002", 
                "456 Oak Ave", "Springfield", "IL", "62702", new DateTime(1990, 8, 22)),
            
            new Customer("Michael", "Brown", "michael.brown@email.com", "+1-555-1003", 
                "789 Pine St", "Springfield", "IL", "62703", new DateTime(1988, 12, 3)),
            
            new Customer("Sarah", "Davis", "sarah.davis@email.com", "+1-555-1004", 
                "321 Elm St", "Springfield", "IL", "62704", new DateTime(1992, 3, 18)),
            
            new Customer("David", "Wilson", "david.wilson@email.com", "+1-555-1005", 
                "654 Maple Ave", "Springfield", "IL", "62705", new DateTime(1987, 9, 7))
        };

        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} customers", customers.Length);
    }
}