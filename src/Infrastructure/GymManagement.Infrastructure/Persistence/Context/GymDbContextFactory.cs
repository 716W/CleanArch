using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymManagement.Infrastructure.Persistence.Context;

public sealed class GymDbContextFactory : IDesignTimeDbContextFactory<GymDbContext>
{
    public GymDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GymDbContext>();
        optionsBuilder.UseSqlite("Data Source=gymmanagement.db");

        return new GymDbContext(optionsBuilder.Options);
    }
}
