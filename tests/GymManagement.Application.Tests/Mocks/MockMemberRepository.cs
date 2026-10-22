
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Mocks;

public class MockMemberRepository
{
    public Mock<IMemberRepository> Mock { get; }
    public MockMemberRepository()
    {
        var members = new List<Member>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Ali",
                LastName = "Test",
                Email = "ali@example.com",
                DateOfBirth = new DateTime(2000, 1, 1),
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Inactive",
                LastName = "Member",
                Email = "inactive@example.com",
                DateOfBirth = new DateTime(1998, 1, 1),
                IsActive = false
            }
        };

        Mock = new Mock<IMemberRepository>();

        Mock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => (IReadOnlyList<Member>)members.ToList());

        Mock.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                members.FirstOrDefault(m => m.Id == id));

        Mock.Setup(r => r.ExistsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string email, Guid? excludeMemberId, CancellationToken _) =>
                members.Any(m => m.Email == email && (!excludeMemberId.HasValue || m.Id != excludeMemberId.Value)));

        Mock.Setup(r => r.AddAsync(
                It.IsAny<Member>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member member, CancellationToken _) =>
            {
                members.Add(member);
                return member;
            });

        Mock.Setup(r => r.UpdateAsync(
                It.IsAny<Member>(),
                It.IsAny<CancellationToken>()))
            .Callback((Member member, CancellationToken _) =>
            {
                var index = members.FindIndex(m => m.Id == member.Id);

                if (index >= 0)
                    members[index] = member;
            })
            .Returns(Task.CompletedTask);

        Mock.Setup(r => r.DeleteAsync(
                It.IsAny<Member>(),
                It.IsAny<CancellationToken>()))
            .Callback((Member member, CancellationToken _) =>
                members.RemoveAll(m => m.Id == member.Id))
            .Returns(Task.CompletedTask);
    }
}