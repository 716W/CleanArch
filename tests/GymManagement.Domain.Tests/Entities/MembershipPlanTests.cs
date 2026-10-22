using GymManagement.Domain.Entities;

namespace GymManagement.Domain.Tests.Entities;

public class MembershipPlanTests
{
    [Fact]
    public void DefaultIsActive_IsTrue()
    {
        var plan = new MembershipPlan();

        Assert.True(plan.IsActive);
    }
}
