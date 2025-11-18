using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POSApi.Infrastructure.Persistence;

public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    public PosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();

        // Use a connection string for design-time migrations
        // This will be overridden by the actual connection string at runtime
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=POSDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new PosDbContext(optionsBuilder.Options);
    }
}
