using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Identity.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.HasData(
            new IdentityUserRole<string>
            {
                RoleId = RoleConfiguration.AdminRoleId,
                UserId = UserConfiguration.AdminUserId
            },
            new IdentityUserRole<string>
            {
                RoleId = RoleConfiguration.UserRoleId,
                UserId = UserConfiguration.RegularUserId
            }
        );
    }
}
