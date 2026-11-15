
using GymManagement.Identity.Context;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GymManagement.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<GymDbContext>>();
            services.RemoveAll<DbContextOptions<GymIdentityDbContext>>();

            services.AddDbContext<GymDbContext>(options =>
                options.UseSqlite("Data Source=gymmanagement_test.db"));

            services.AddDbContext<GymIdentityDbContext>(options =>
                options.UseSqlite(
                    "Data Source=gymmanagement_test.db",
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_Identity")));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<GymDbContext>();
        var identityContext = scope.ServiceProvider
            .GetRequiredService<GymIdentityDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        await identityContext.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<GymDbContext>();
        var identityContext = scope.ServiceProvider
            .GetRequiredService<GymIdentityDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        await identityContext.Database.MigrateAsync();
    }
}
