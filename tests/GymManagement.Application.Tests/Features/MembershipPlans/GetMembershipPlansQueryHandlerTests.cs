using AutoMapper;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using Moq;

namespace GymManagement.Application.Tests.Features.MembershipPlans;

public class GetMembershipPlansQueryHandlerTests
{
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;
    private readonly IMapper _mapper;

    public GetMembershipPlansQueryHandlerTests()
    {
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithoutFilter_ReturnsAllMappedMembershipPlans()
    {
        // Arrange
        var handler = new GetMembershipPlansQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var query = new GetMembershipPlansQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var activePlan = result.First(p => p.Id == Guid.Parse("33333333-3333-3333-3333-333333333333"));
        Assert.Equal("Monthly", activePlan.Name);
        Assert.Equal(30, activePlan.DurationInDays);
        Assert.Equal(100m, activePlan.Price);
        Assert.True(activePlan.IsActive);
        Assert.Equal(5, activePlan.MaxFreezeDays);
        Assert.Equal(1, activePlan.DisplayOrder);

        var inactivePlan = result.First(p => p.Id == Guid.Parse("44444444-4444-4444-4444-444444444444"));
        Assert.Equal("Inactive Plan", inactivePlan.Name);
        Assert.Equal(90, inactivePlan.DurationInDays);
        Assert.Equal(250m, inactivePlan.Price);
        Assert.False(inactivePlan.IsActive);
        Assert.Equal(10, inactivePlan.MaxFreezeDays);
        Assert.Equal(2, inactivePlan.DisplayOrder);
    }

    [Fact]
    public async Task Handle_ActiveOnlyTrue_ReturnsOnlyActiveMembershipPlans()
    {
        // Arrange
        var handler = new GetMembershipPlansQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var query = new GetMembershipPlansQuery(ActiveOnly: true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result[0].IsActive);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), result[0].Id);
        Assert.Equal("Monthly", result[0].Name);
    }

    [Fact]
    public async Task Handle_ActiveOnlyFalse_ReturnsAllMembershipPlans()
    {
        // Arrange
        var handler = new GetMembershipPlansQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var query = new GetMembershipPlansQuery(ActiveOnly: false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPlansExist_ReturnsEmptyList()
    {
        // Arrange
        _mockMembershipPlanRepository.Mock
            .Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MembershipPlan>());

        var handler = new GetMembershipPlansQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var query = new GetMembershipPlansQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ValidQuery_PassesParametersAndCancellationTokenToRepository()
    {
        // Arrange
        var handler = new GetMembershipPlansQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var query = new GetMembershipPlansQuery(ActiveOnly: true);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetAllAsync(true, cancellationToken),
            Times.Once);
    }
}
