using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.MembershipPlans.Commands.ActivateMembershipPlan;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.MembershipPlans;

public class ActivateMembershipPlanCommandHandlerTests
{
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;

    public ActivateMembershipPlanCommandHandlerTests()
    {
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();
    }

    [Fact]
    public async Task Handle_ExistingInactivePlan_SetsIsActiveTrueAndReturnsTrue()
    {
        // Arrange
        var handler = new ActivateMembershipPlanCommandHandler(_mockMembershipPlanRepository.Mock.Object);
        var inactivePlanId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var command = new ActivateMembershipPlanCommand(inactivePlanId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(inactivePlanId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<MembershipPlan>(p => p.Id == inactivePlanId && p.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var updatedPlan = await _mockMembershipPlanRepository.Mock.Object.GetByIdAsync(inactivePlanId);
        Assert.NotNull(updatedPlan);
        Assert.True(updatedPlan.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingAlreadyActivePlan_KeepsIsActiveTrueAndReturnsTrue()
    {
        // Arrange
        var handler = new ActivateMembershipPlanCommandHandler(_mockMembershipPlanRepository.Mock.Object);
        var activePlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var command = new ActivateMembershipPlanCommand(activePlanId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<MembershipPlan>(p => p.Id == activePlanId && p.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var updatedPlan = await _mockMembershipPlanRepository.Mock.Object.GetByIdAsync(activePlanId);
        Assert.NotNull(updatedPlan);
        Assert.True(updatedPlan.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingPlan_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new ActivateMembershipPlanCommandHandler(_mockMembershipPlanRepository.Mock.Object);
        var planId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var command = new ActivateMembershipPlanCommand(planId);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(planId, cancellationToken),
            Times.Once);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<MembershipPlan>(p => p.Id == planId && p.IsActive),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new ActivateMembershipPlanCommandHandler(_mockMembershipPlanRepository.Mock.Object);
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new ActivateMembershipPlanCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Membership plan with id '{nonExistentId}' was not found.", exception.Message);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
