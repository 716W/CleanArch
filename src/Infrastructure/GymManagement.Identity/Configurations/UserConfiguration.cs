using GymManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Identity.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public const string AdminUserId = "8e445865-a24d-4543-a6c6-9443d048cdb9";
    public const string RegularUserId = "9f556976-b35e-4654-b7d7-0554e159dec0";

    // Pre-computed PBKDF2 Identity password hashes for deterministic model building:
    // Admin@123!
    private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEO/G2GtXcEGiF6YXTrFzm+We93N4fmM1oeS6RUIlJq83oIzdN7P7l/NU3HNoNINILw==";
    // User@123!
    private const string RegularPasswordHash = "AQAAAAIAAYagAAAAEHy6O4LY7smx6coerg2TbzXRS+nRcw86ojCHxSTaHvm4RlrbHuch8/hmLZ+767JwHQ==";

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        var adminUser = new ApplicationUser
        {
            Id = AdminUserId,
            Email = "admin@gymmanagement.com",
            NormalizedEmail = "ADMIN@GYMMANAGEMENT.COM",
            UserName = "admin@gymmanagement.com",
            NormalizedUserName = "ADMIN@GYMMANAGEMENT.COM",
            FirstName = "System",
            LastName = "Admin",
            EmailConfirmed = true,
            PasswordHash = AdminPasswordHash,
            SecurityStamp = "d6e3c09b-60a2-4a0b-9273-0f7f3e8f8101",
            ConcurrencyStamp = "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"
        };

        var regularUser = new ApplicationUser
        {
            Id = RegularUserId,
            Email = "user@gymmanagement.com",
            NormalizedEmail = "USER@GYMMANAGEMENT.COM",
            UserName = "user@gymmanagement.com",
            NormalizedUserName = "USER@GYMMANAGEMENT.COM",
            FirstName = "Regular",
            LastName = "User",
            EmailConfirmed = true,
            PasswordHash = RegularPasswordHash,
            SecurityStamp = "e7f4d10c-71b3-5b1c-0384-1a8a4f9a9212",
            ConcurrencyStamp = "b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"
        };

        builder.HasData(adminUser, regularUser);
    }
}
