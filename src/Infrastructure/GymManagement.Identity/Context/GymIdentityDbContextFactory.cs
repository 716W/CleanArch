using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymManagement.Identity.Context;

public sealed class GymIdentityDbContextFactory : IDesignTimeDbContextFactory<GymIdentityDbContext>
{
    public GymIdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GymIdentityDbContext>();
        optionsBuilder.UseSqlite(
            "Data Source=gymmanagement.db",
            b => b.MigrationsHistoryTable("__EFMigrationsHistory_Identity"));

        return new GymIdentityDbContext(optionsBuilder.Options);
    }
}
