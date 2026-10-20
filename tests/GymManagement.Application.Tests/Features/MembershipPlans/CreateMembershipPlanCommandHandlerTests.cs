using AutoMapper;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.MembershipPlans;

public class CreateMembershipPlanCommandHandlerTests
{
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;
    private readonly IMapper _mapper;

    public CreateMembershipPlanCommandHandlerTests()
    {
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsMembershipPlanResponse()
    {
        // Arrange
        var handler = new CreateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipPlanCommand(
            "Annual",
            365,
            800m,
            30,
            3);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.DurationInDays, result.DurationInDays);
        Assert.Equal(command.Price, result.Price);
        Assert.Equal(command.MaxFreezeDays, result.MaxFreezeDays);
        Assert.Equal(command.DisplayOrder, result.DisplayOrder);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsyncWithExpectedPlanAndCancellationToken()
    {
        // Arrange
        var handler = new CreateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipPlanCommand(
            "Quarterly",
            90,
            240m,
            10,
            2);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<MembershipPlan>(p =>
                    p.Name == command.Name &&
                    p.DurationInDays == command.DurationInDays &&
                    p.Price == command.Price &&
                    p.MaxFreezeDays == command.MaxFreezeDays &&
                    p.DisplayOrder == command.DisplayOrder &&
                    p.IsActive &&
                    p.Id != Guid.Empty),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsPlanInRepository()
    {
        // Arrange
        var handler = new CreateMembershipPlanCommandHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipPlanCommand(
            "Day Pass",
            1,
            15m,
            0,
            10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var persisted = await _mockMembershipPlanRepository.Mock.Object.GetByIdAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(result.Id, persisted.Id);
        Assert.Equal(command.Name, persisted.Name);
        Assert.Equal(command.DurationInDays, persisted.DurationInDays);
        Assert.Equal(command.Price, persisted.Price);
        Assert.Equal(command.MaxFreezeDays, persisted.MaxFreezeDays);
        Assert.Equal(command.DisplayOrder, persisted.DisplayOrder);
        Assert.True(persisted.IsActive);
    }
}
