using GymManagement.Domain.Entities;

namespace GymManagement.Domain.Tests.Entities;

public class MemberTests
{
    [Fact]
    public void DefaultIsActive_IsTrue()
    {
        var member = new Member();

        Assert.True(member.IsActive);
    }

    [Fact]
    public void DefaultJoinedDate_IsWithinConstructionTimeWindow()
    {
        var before = DateTime.UtcNow;
        var member = new Member();
        var after = DateTime.UtcNow;

        Assert.InRange(member.JoinedDate, before, after);
    }
}
