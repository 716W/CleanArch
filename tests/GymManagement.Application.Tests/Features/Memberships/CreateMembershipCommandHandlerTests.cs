using AutoMapper;
using GymManagement.Application.Exceptions;
using GymManagement.Application.Features.Memberships.Commands.CreateMemberships;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Application.Mapping;
using GymManagement.Application.Tests.Mocks;
using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using Moq;

namespace GymManagement.Application.Tests.Features.Memberships;

public class CreateMembershipCommandHandlerTests
{
    private readonly MockMemberRepository _mockMemberRepository;
    private readonly MockMembershipPlanRepository _mockMembershipPlanRepository;
    private readonly MockMembershipRepository _mockMembershipRepository;

    private readonly IMapper _mapper;

    public CreateMembershipCommandHandlerTests()
    {
        _mockMemberRepository = new MockMemberRepository();
        _mockMembershipPlanRepository = new MockMembershipPlanRepository();
        _mockMembershipRepository = new MockMembershipRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task CreateMembership_ValidCommand_ReturnsMembershipResponse()
    {
        // Arrange
        var memberRepository = _mockMemberRepository.Mock.Object;
        var planRepository = _mockMembershipPlanRepository.Mock.Object;
        var membershipRepository = _mockMembershipRepository.Mock.Object;

        var handler = new CreateMembershipCommandHandler(
            memberRepository,
            planRepository,
            membershipRepository,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-1001"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.MemberId, result.MemberId);
        Assert.Equal(command.MembershipPlanId, result.MembershipPlanId);
        Assert.Equal(new DateTime(2026, 11, 17), result.EndDate);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesExpectedMembershipAndCancellationTokenToAddAsync()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var startDate = new DateTime(2026, 10, 18);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = startDate,
            PaymentTransactionId = "TX-9999"
        };

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m =>
                    m.Id != Guid.Empty &&
                    m.MemberId == memberId &&
                    m.MembershipPlanId == planId &&
                    m.StartDate == startDate.Date &&
                    m.EndDate == startDate.Date.AddDays(30) &&
                    m.Status == MembershipStatus.Active &&
                    m.PaymentTransactionId == "TX-9999"),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithWhitespaceInPaymentTransactionId_TrimsPaymentTransactionId()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "   TX-TRIM-ME   "
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m => m.PaymentTransactionId == "TX-TRIM-ME"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommandWithTimeComponentInStartDate_NormalizesStartDateToDateOnly()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 10, 18, 15, 30, 45),
            PaymentTransactionId = "TX-TIME-TEST"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 10, 18, 0, 0, 0), result.StartDate);
        Assert.Equal(new DateTime(2026, 11, 17, 0, 0, 0), result.EndDate);
        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m =>
                    m.StartDate == new DateTime(2026, 10, 18) &&
                    m.EndDate == new DateTime(2026, 11, 17)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationTokenToAllRepositories()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var startDate = new DateTime(2026, 10, 18);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = startDate,
            PaymentTransactionId = "TX-TOKEN"
        };

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMemberRepository.Mock.Verify(
            repo => repo.GetByIdAsync(memberId, cancellationToken),
            Times.Once);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(planId, cancellationToken),
            Times.Once);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.HasConflictingMembershipAsync(memberId, startDate.Date, cancellationToken),
            Times.Once);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var nonExistentMemberId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new CreateMembershipCommand
        {
            MemberId = nonExistentMemberId,
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NOTFOUND"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Member with id '{nonExistentMemberId}' was not found.", exception.Message);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveMember_ThrowsBusinessRuleException()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var inactiveMemberId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var command = new CreateMembershipCommand
        {
            MemberId = inactiveMemberId,
            MembershipPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-INACTIVE-MEMBER"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Member is inactive.", exception.Message);

        _mockMembershipPlanRepository.Mock.Verify(
            repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MembershipPlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var nonExistentPlanId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = nonExistentPlanId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-PLAN-NOTFOUND"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Membership plan with id '{nonExistentPlanId}' was not found.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveMembershipPlan_ThrowsBusinessRuleException()
    {
        // Arrange
        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var inactivePlanId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = inactivePlanId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-INACTIVE-PLAN"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Membership plan is inactive.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public async Task Handle_PlanWithZeroOrNegativeDuration_ThrowsBusinessRuleException(int durationInDays)
    {
        // Arrange
        var invalidDurationPlanId = Guid.NewGuid();
        await _mockMembershipPlanRepository.Mock.Object.AddAsync(new MembershipPlan
        {
            Id = invalidDurationPlanId,
            Name = "Invalid Duration Plan",
            DurationInDays = durationInDays,
            Price = 100m,
            IsActive = true
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = invalidDurationPlanId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-INVALID-DURATION"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Membership plan configuration is invalid.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PlanWithNegativePrice_ThrowsBusinessRuleException()
    {
        // Arrange
        var negativePricePlanId = Guid.NewGuid();
        await _mockMembershipPlanRepository.Mock.Object.AddAsync(new MembershipPlan
        {
            Id = negativePricePlanId,
            Name = "Negative Price Plan",
            DurationInDays = 30,
            Price = -50m,
            IsActive = true
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            MembershipPlanId = negativePricePlanId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NEGATIVE-PRICE"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Membership plan configuration is invalid.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConflictingActiveMembership_ThrowsConflictException()
    {
        // Arrange
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Existing active membership overlapping startDate (2026-10-18)
        await _mockMembershipRepository.Mock.Object.AddAsync(new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-EXISTING-ACTIVE"
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NEW-CONFLICT"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("The member already has a conflicting membership.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m => m.PaymentTransactionId == "TX-NEW-CONFLICT"),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConflictingFrozenMembership_ThrowsConflictException()
    {
        // Arrange
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Existing frozen membership overlapping startDate (2026-10-18)
        await _mockMembershipRepository.Mock.Object.AddAsync(new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 31),
            Status = MembershipStatus.Frozen,
            PaymentTransactionId = "TX-EXISTING-FROZEN"
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NEW-FROZEN-CONFLICT"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("The member already has a conflicting membership.", exception.Message);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m => m.PaymentTransactionId == "TX-NEW-FROZEN-CONFLICT"),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingExpiredMembership_DoesNotConflictAndCreatesMembership()
    {
        // Arrange
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Existing membership with Status = Expired, even if EndDate is after or equal to StartDate
        await _mockMembershipRepository.Mock.Object.AddAsync(new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 9, 1),
            EndDate = new DateTime(2026, 10, 31),
            Status = MembershipStatus.Expired,
            PaymentTransactionId = "TX-EXISTING-EXPIRED"
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NEW-AFTER-EXPIRED"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.MemberId);
        Assert.Equal(planId, result.MembershipPlanId);
        Assert.Equal("Active", result.Status);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m => m.PaymentTransactionId == "TX-NEW-AFTER-EXPIRED"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingActiveMembershipEndedBeforeStartDate_DoesNotConflictAndCreatesMembership()
    {
        // Arrange
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var planId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Existing active membership ended before new StartDate (2026-10-10 < 2026-10-18)
        await _mockMembershipRepository.Mock.Object.AddAsync(new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 9, 10),
            EndDate = new DateTime(2026, 10, 10),
            Status = MembershipStatus.Active,
            PaymentTransactionId = "TX-OLD-ENDED"
        });

        var handler = new CreateMembershipCommandHandler(
            _mockMemberRepository.Mock.Object,
            _mockMembershipPlanRepository.Mock.Object,
            _mockMembershipRepository.Mock.Object,
            _mapper);

        var command = new CreateMembershipCommand
        {
            MemberId = memberId,
            MembershipPlanId = planId,
            StartDate = new DateTime(2026, 10, 18),
            PaymentTransactionId = "TX-NEW-NON-OVERLAPPING"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.MemberId);
        Assert.Equal(planId, result.MembershipPlanId);
        Assert.Equal("Active", result.Status);

        _mockMembershipRepository.Mock.Verify(
            repo => repo.AddAsync(
                It.Is<Membership>(m => m.PaymentTransactionId == "TX-NEW-NON-OVERLAPPING"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
