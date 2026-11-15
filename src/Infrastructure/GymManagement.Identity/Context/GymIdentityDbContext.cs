using GymManagement.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Identity.Context;

public class GymIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public GymIdentityDbContext(DbContextOptions<GymIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GymIdentityDbContext).Assembly);
    }
}
