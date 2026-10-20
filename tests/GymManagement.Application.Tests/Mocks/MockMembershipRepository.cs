
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using Moq;

namespace GymManagement.Application.Tests.Mocks;

public class MockMembershipRepository
{
    public Mock<IMembershipRepository> Mock { get; }

    public MockMembershipRepository()
    {
        var memberships = new List<Membership>();

        Mock = new Mock<IMembershipRepository>();

        Mock.Setup(r => r.HasConflictingMembershipAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid memberId, DateTime startDate, CancellationToken _) =>
                memberships.Any(m =>
                    m.MemberId == memberId &&
                    (m.Status == MembershipStatus.Active ||
                     m.Status == MembershipStatus.Frozen) &&
                    m.EndDate.Date >= startDate.Date));

        Mock.Setup(r => r.AddAsync(
                It.IsAny<Membership>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Membership membership, CancellationToken _) =>
            {
                memberships.Add(membership);
                return membership;
            });

        Mock.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                memberships.FirstOrDefault(m => m.Id == id));

        Mock.Setup(r => r.GetByMemberIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid memberId, CancellationToken _) =>
                (IReadOnlyList<Membership>)memberships
                    .Where(m => m.MemberId == memberId)
                    .ToList());
    }
}
