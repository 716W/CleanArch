using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipById;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using Moq;

namespace GymManagement.Application.Tests.Features.Memberships;

public class GetMembershipByIdQueryHandlerTests
{
    private readonly MockMembershipRepository _mockMembershipRepository;
    private readonly IMapper _mapper;

    public GetMembershipByIdQueryHandlerTests()
    {
        _mockMembershipRepository = new MockMembershipRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingActiveMembership_ReturnsMappedMembershipResponse()
    {
        // Arrange
        var handler = new GetMembershipByIdQueryHandler(
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-ACTIVE"
        };
        await _mockMembershipRepository.Mock.Object.AddAsync(membership);

        var query = new GetMembershipByIdQuery(membership.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(membership.Id, result.Id);
        Assert.Equal(membership.MemberId, result.MemberId);
        Assert.Equal(membership.MembershipPlanId, result.MembershipPlanId);
        Assert.Equal(membership.StartDate, result.StartDate);
        Assert.Equal(membership.EndDate, result.EndDate);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Handle_ExistingFrozenMembership_ReturnsMappedMembershipResponseWithFrozenStatus()
    {
        // Arrange
        var handler = new GetMembershipByIdQueryHandler(
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 3, 3),
            Status = MembershipStatus.Frozen,
            PaymentTransactionId = "TX-FROZEN"
        };
        await _mockMembershipRepository.Mock.Object.AddAsync(membership);

        var query = new GetMembershipByIdQuery(membership.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(membership.Id, result.Id);
        Assert.Equal("Frozen", result.Status);
    }

    [Fact]
    public async Task Handle_ExistingExpiredMembership_ReturnsMappedMembershipResponseWithExpiredStatus()
    {
        // Arrange
        var handler = new GetMembershipByIdQueryHandler(
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            Status = MembershipStatus.Expired,
            PaymentTransactionId = "TX-EXPIRED"
        };
        await _mockMembershipRepository.Mock.Object.AddAsync(membership);

        var query = new GetMembershipByIdQuery(membership.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(membership.Id, result.Id);
        Assert.Equal("Expired", result.Status);
    }

    [Fact]
    public async Task Handle_MembershipNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetMembershipByIdQueryHandler(
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var query = new GetMembershipByIdQuery(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Membership with id '{nonExistentId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_ExistingMembership_PassesCancellationTokenToRepository()
    {
        // Arrange
        var handler = new GetMembershipByIdQueryHandler(
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-CANCEL-TOKEN"
        };
        await _mockMembershipRepository.Mock.Object.AddAsync(membership);

        var query = new GetMembershipByIdQuery(membership.Id);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMembershipRepository.Mock.Verify(
            repo => repo.GetByIdAsync(membership.Id, cancellationToken),
            Times.Once);
    }
}

