using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.MembershipPlans;

public class UpdateMembershipPlanCommandHandlerTests
{
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;
    private readonly IMapper _mapper;

    public UpdateMembershipPlanCommandHandlerTests()
    {
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingActivePlan_ReturnsUpdatedMembershipPlanResponse()
    {
        // Arrange
        var handler = new UpdateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var existingPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var command = new UpdateMembershipPlanCommand(
            existingPlanId,
            "Monthly Pro",
            30,
            120m,
            7,
            5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingPlanId, result.Id);
        Assert.Equal("Monthly Pro", result.Name);
        Assert.Equal(30, result.DurationInDays);
        Assert.Equal(120m, result.Price);
        Assert.Equal(7, result.MaxFreezeDays);
        Assert.Equal(5, result.DisplayOrder);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingInactivePlan_PreservesInactiveStatus()
    {
        // Arrange
        var handler = new UpdateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var existingPlanId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var command = new UpdateMembershipPlanCommand(
            existingPlanId,
            "Quarterly Standard",
            90,
            220m,
            12,
            3);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingPlanId, result.Id);
        Assert.Equal("Quarterly Standard", result.Name);
        Assert.Equal(90, result.DurationInDays);
        Assert.Equal(220m, result.Price);
        Assert.Equal(12, result.MaxFreezeDays);
        Assert.Equal(3, result.DisplayOrder);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingPlan_CallsRepositoryUpdateAsyncAndPersistsChanges()
    {
        // Arrange
        var handler = new UpdateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var existingPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var command = new UpdateMembershipPlanCommand(
            existingPlanId,
            "Monthly Premium",
            30,
            150m,
            8,
            1);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingPlanId, cancellationToken),
            Times.Once);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(
                It.Is<MembershipPlan>(p =>
                    p.Id == existingPlanId &&
                    p.Name == command.Name &&
                    p.DurationInDays == command.DurationInDays &&
                    p.Price == command.Price &&
                    p.MaxFreezeDays == command.MaxFreezeDays &&
                    p.DisplayOrder == command.DisplayOrder &&
                    p.IsActive),
                cancellationToken),
            Times.Once);

        var persisted = await _mockMembershipPlanRepository.Mock.Object.GetByIdAsync(existingPlanId);
        Assert.NotNull(persisted);
        Assert.Equal("Monthly Premium", persisted.Name);
        Assert.Equal(150m, persisted.Price);
        Assert.Equal(8, persisted.MaxFreezeDays);
        Assert.True(persisted.IsActive);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new UpdateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new UpdateMembershipPlanCommand(
            nonExistentId,
            "Non Existent",
            30,
            50m,
            0,
            1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Membership plan with id '{nonExistentId}' was not found.", exception.Message);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.UpdateAsync(It.IsAny<MembershipPlan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
