using GymManagement.Domain.Enums;

namespace GymManagement.Domain.Tests.Enums;

public class MembershipStatusTests
{
    [Fact]
    public void MembershipStatus_HasExpectedValues()
    {
        Assert.Equal(1, (int)MembershipStatus.Active);
        Assert.Equal(2, (int)MembershipStatus.Frozen);
        Assert.Equal(3, (int)MembershipStatus.Expired);
    }
}
