using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Tests.Entities;

public class MembershipTests
{
    [Fact]
    public void DefaultStatus_IsActive()
    {
        var membership = new Membership();

        Assert.Equal(MembershipStatus.Active, membership.Status);
    }
}
