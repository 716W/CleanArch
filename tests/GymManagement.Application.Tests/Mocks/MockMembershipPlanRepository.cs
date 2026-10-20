
using GymManagement.Application.Contracts.Persistence;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Mocks;

public class MockMembershipPlanRepository
{
    public Mock<IMembershipPlanRepository> Mock { get; }

    public MockMembershipPlanRepository()
    {
        var plans = new List<MembershipPlan>
        {
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Monthly",
                DurationInDays = 30,
                Price = 100m,
                IsActive = true,
                MaxFreezeDays = 5,
                DisplayOrder = 1
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Inactive Plan",
                DurationInDays = 90,
                Price = 250m,
                IsActive = false,
                MaxFreezeDays = 10,
                DisplayOrder = 2
            }
        };

        Mock = new Mock<IMembershipPlanRepository>();

        Mock.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                plans.FirstOrDefault(p => p.Id == id));

        Mock.Setup(r => r.GetAllAsync(
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool? activeOnly, CancellationToken _) =>
            {
                IEnumerable<MembershipPlan> query = plans;

                if (activeOnly == true)
                    query = query.Where(p => p.IsActive);

                return (IReadOnlyList<MembershipPlan>)query.ToList();
            });

        Mock.Setup(r => r.AddAsync(
                It.IsAny<MembershipPlan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MembershipPlan plan, CancellationToken _) =>
            {
                plans.Add(plan);
                return plan;
            });

        Mock.Setup(r => r.UpdateAsync(
                It.IsAny<MembershipPlan>(),
                It.IsAny<CancellationToken>()))
            .Callback((MembershipPlan plan, CancellationToken _) =>
            {
                var index = plans.FindIndex(p => p.Id == plan.Id);

                if (index >= 0)
                    plans[index] = plan;
            })
            .Returns(Task.CompletedTask);
    }
}
