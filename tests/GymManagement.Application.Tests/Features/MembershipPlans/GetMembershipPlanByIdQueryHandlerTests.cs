using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using Moq;

namespace GymManagement.Application.Tests.Features.MembershipPlans;

public class GetMembershipPlanByIdQueryHandlerTests
{
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;
    private readonly IMapper _mapper;

    public GetMembershipPlanByIdQueryHandlerTests()
    {
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingActivePlan_ReturnsMappedMembershipPlanResponse()
    {
        // Arrange
        var handler = new GetMembershipPlanByIdQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var existingPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var query = new GetMembershipPlanByIdQuery(existingPlanId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingPlanId, result.Id);
        Assert.Equal("Monthly", result.Name);
        Assert.Equal(30, result.DurationInDays);
        Assert.Equal(100m, result.Price);
        Assert.True(result.IsActive);
        Assert.Equal(5, result.MaxFreezeDays);
        Assert.Equal(1, result.DisplayOrder);
    }

    [Fact]
    public async Task Handle_ExistingInactivePlan_ReturnsMappedMembershipPlanResponse()
    {
        // Arrange
        var handler = new GetMembershipPlanByIdQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var inactivePlanId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var query = new GetMembershipPlanByIdQuery(inactivePlanId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(inactivePlanId, result.Id);
        Assert.Equal("Inactive Plan", result.Name);
        Assert.Equal(90, result.DurationInDays);
        Assert.Equal(250m, result.Price);
        Assert.False(result.IsActive);
        Assert.Equal(10, result.MaxFreezeDays);
        Assert.Equal(2, result.DisplayOrder);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetMembershipPlanByIdQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var query = new GetMembershipPlanByIdQuery(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Membership plan with id '{nonExistentId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_ExistingPlan_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new GetMembershipPlanByIdQueryHandler(
            _mockMembershipPlanRepository.Mock.Object,
            _mapper);

        var existingPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var query = new GetMembershipPlanByIdQuery(existingPlanId);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(existingPlanId, cancellationToken),
            Times.Once);
    }
}
