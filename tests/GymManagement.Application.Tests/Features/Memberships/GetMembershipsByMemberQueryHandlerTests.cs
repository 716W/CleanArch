using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Application.Features.Memberships.Queries.GetMembershipsByMember;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using Moq;

namespace GymManagement.Application.Tests.Features.Memberships;

public class GetMembershipsByMemberQueryHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly MockMembershipRepository _mockMembershipRepository;
    private readonly IMapper _mapper;

    public GetMembershipsByMemberQueryHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();
        _mockMembershipRepository = new MockMembershipRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingMemberWithMultipleMemberships_ReturnsMappedMembershipList()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var membership1 = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            Status = MembershipStatus.Expired,
            PaymentTransactionId = "TX-LIST-1"
        };
        var membership2 = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 3, 3),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-LIST-2"
        };

        await _mockMembershipRepository.Mock.Object.AddAsync(membership1);
        await _mockMembershipRepository.Mock.Object.AddAsync(membership2);

        var query = new GetMembershipsByMemberQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var item1 = result.First(m => m.Id == membership1.Id);
        Assert.Equal(memberId, item1.MemberId);
        Assert.Equal(planId, item1.MembershipPlanId);
        Assert.Equal(membership1.StartDate, item1.StartDate);
        Assert.Equal(membership1.EndDate, item1.EndDate);
        Assert.Equal("Expired", item1.Status);

        var item2 = result.First(m => m.Id == membership2.Id);
        Assert.Equal(memberId, item2.MemberId);
        Assert.Equal(planId, item2.MembershipPlanId);
        Assert.Equal(membership2.StartDate, item2.StartDate);
        Assert.Equal(membership2.EndDate, item2.EndDate);
        Assert.Equal("Active", item2.Status);
    }

    [Fact]
    public async Task Handle_ExistingMemberWithFrozenMembership_ReturnsMappedMembershipWithFrozenStatus()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 3, 3),
            Status = MembershipStatus.Frozen,
            PaymentTransactionId = "TX-FROZEN-LIST"
        };
        await _mockMembershipRepository.Mock.Object.AddAsync(membership);

        var query = new GetMembershipsByMemberQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var item = Assert.Single(result);
        Assert.Equal("Frozen", item.Status);
    }

    [Fact]
    public async Task Handle_ExistingMemberWithNoMemberships_ReturnsEmptyList()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new GetMembershipsByMemberQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var nonExistentMemberId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var query = new GetMembershipsByMemberQuery(nonExistentMemberId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Member with id '{nonExistentMemberId}' was not found.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.GetByMemberIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingMember_PassesCancellationTokenToRepositories()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new GetMembershipsByMemberQuery(memberId);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(memberId, cancellationToken),
            Times.Once);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.GetByMemberIdAsync(memberId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleMembersInSystem_OnlyReturnsMembershipsForRequestedMember()
    {
        // Arrange
        var handler = new GetMembershipsByMemberQueryHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var member1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var member2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var membershipForMember1 = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = member1Id,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-M1"
        };

        var membershipForMember2 = new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = member2Id,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-M2"
        };

        await _mockMembershipRepository.Mock.Object.AddAsync(membershipForMember1);
        await _mockMembershipRepository.Mock.Object.AddAsync(membershipForMember2);

        var query = new GetMembershipsByMemberQuery(member1Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var item = Assert.Single(result);
        Assert.Equal(membershipForMember1.Id, item.Id);
        Assert.Equal(member1Id, item.MemberId);
    }
}

