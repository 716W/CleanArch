using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Identity.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public const string AdminRoleId = "cbc43a8e-f7bb-4445-baaf-1add431ffbbf";
    public const string UserRoleId = "cac43a6e-f7bb-4448-baaf-1add431ccbbf";

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = AdminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "c7b013f0-5201-4317-abd8-c211f91b7330"
            },
            new IdentityRole
            {
                Id = UserRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "c7b013f0-5201-4317-abd8-c211f91b7331"
            }
        );
    }
}
