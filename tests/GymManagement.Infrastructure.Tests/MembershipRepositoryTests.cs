using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Tests;

public class MembershipRepositoryTests : InfrastructureTestBase
{
    private MembershipRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new MembershipRepository(Context);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenActiveMembershipWithOverlappingEndDate_ShouldReturnTrue()
    {
        // Arrange
        var member = CreateMember("active.conflict@example.com");
        var plan = CreatePlan("Plan 1");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);

        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 5, 1),
            endDate: new DateTime(2026, 5, 31),
            status: MembershipStatus.Active);
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();

        var requestedStartDate = new DateTime(2026, 5, 15);

        // Act
        var result = await _repository.HasConflictingMembershipAsync(member.Id, requestedStartDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenFrozenMembershipWithOverlappingEndDate_ShouldReturnTrue()
    {
        // Arrange
        var member = CreateMember("frozen.conflict@example.com");
        var plan = CreatePlan("Plan 2");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);

        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 5, 1),
            endDate: new DateTime(2026, 5, 31),
            status: MembershipStatus.Frozen);
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();

        var requestedStartDate = new DateTime(2026, 5, 15);

        // Act
        var result = await _repository.HasConflictingMembershipAsync(member.Id, requestedStartDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenExpiredMembershipWithOverlappingEndDate_ShouldReturnFalse()
    {
        // Arrange
        var member = CreateMember("expired.conflict@example.com");
        var plan = CreatePlan("Plan 3");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);

        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 5, 1),
            endDate: new DateTime(2026, 5, 31),
            status: MembershipStatus.Expired);
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();

        var requestedStartDate = new DateTime(2026, 5, 15);

        // Act
        var result = await _repository.HasConflictingMembershipAsync(member.Id, requestedStartDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenActiveMembershipEndsBeforeRequestedStartDate_ShouldReturnFalse()
    {
        // Arrange
        var member = CreateMember("ended.before@example.com");
        var plan = CreatePlan("Plan 4");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);

        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 4, 1),
            endDate: new DateTime(2026, 4, 30),
            status: MembershipStatus.Active);
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();

        var requestedStartDate = new DateTime(2026, 5, 1);

        // Act
        var result = await _repository.HasConflictingMembershipAsync(member.Id, requestedStartDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenEndDateEqualsRequestedStartDate_ShouldFollowRepositorySemanticsAndReturnTrue()
    {
        // Arrange: Repository checks m.EndDate >= startDate, so EndDate == requestedStartDate evaluates to true.
        var member = CreateMember("equal.date@example.com");
        var plan = CreatePlan("Plan 5");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);

        var boundaryDate = new DateTime(2026, 5, 31);
        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 5, 1),
            endDate: boundaryDate,
            status: MembershipStatus.Active);
        await Context.Memberships.AddAsync(membership);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.HasConflictingMembershipAsync(member.Id, boundaryDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasConflictingMembershipAsync_WhenConflictBelongsToAnotherMember_ShouldReturnFalse()
    {
        // Arrange
        var memberA = CreateMember("member.a@example.com");
        var memberB = CreateMember("member.b@example.com");
        var plan = CreatePlan("Plan 6");
        await Context.Members.AddRangeAsync(memberA, memberB);
        await Context.MembershipPlans.AddAsync(plan);

        var membershipA = CreateMembership(
            memberA.Id,
            plan.Id,
            startDate: new DateTime(2026, 5, 1),
            endDate: new DateTime(2026, 5, 31),
            status: MembershipStatus.Active);
        await Context.Memberships.AddAsync(membershipA);
        await Context.SaveChangesAsync();

        var requestedStartDate = new DateTime(2026, 5, 15);

        // Act
        var result = await _repository.HasConflictingMembershipAsync(memberB.Id, requestedStartDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByMemberIdAsync_ShouldReturnMembershipsOrderedByDescendingStartDate()
    {
        // Arrange
        var targetMember = CreateMember("target.member@example.com");
        var otherMember = CreateMember("other.member@example.com");
        var plan = CreatePlan("Plan 7");
        await Context.Members.AddRangeAsync(targetMember, otherMember);
        await Context.MembershipPlans.AddAsync(plan);

        var m1 = CreateMembership(targetMember.Id, plan.Id, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));
        var m2 = CreateMembership(targetMember.Id, plan.Id, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        var m3 = CreateMembership(targetMember.Id, plan.Id, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
        var otherMembership = CreateMembership(otherMember.Id, plan.Id, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        await Context.Memberships.AddRangeAsync(m1, m2, m3, otherMembership);
        await Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByMemberIdAsync(targetMember.Id);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, m => Assert.Equal(targetMember.Id, m.MemberId));
        Assert.Equal(new DateTime(2026, 3, 1), results[0].StartDate);
        Assert.Equal(new DateTime(2026, 2, 1), results[1].StartDate);
        Assert.Equal(new DateTime(2026, 1, 1), results[2].StartDate);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistMembership_AndGetByIdAsync_ShouldRetrieveIt()
    {
        // Arrange
        var member = CreateMember("add.member@example.com");
        var plan = CreatePlan("Add Plan");
        await Context.Members.AddAsync(member);
        await Context.MembershipPlans.AddAsync(plan);
        await Context.SaveChangesAsync();

        var membership = CreateMembership(
            member.Id,
            plan.Id,
            startDate: new DateTime(2026, 6, 1),
            endDate: new DateTime(2026, 6, 30),
            status: MembershipStatus.Active,
            paymentTransactionId: "txn_persist_999");

        // Act
        var added = await _repository.AddAsync(membership);

        // Assert
        Assert.Equal(membership.Id, added.Id);

        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MembershipRepository(cleanContext);
        var retrieved = await cleanRepo.GetByIdAsync(membership.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(membership.Id, retrieved.Id);
        Assert.Equal(member.Id, retrieved.MemberId);
        Assert.Equal(plan.Id, retrieved.MembershipPlanId);
        Assert.Equal(new DateTime(2026, 6, 1), retrieved.StartDate);
        Assert.Equal(new DateTime(2026, 6, 30), retrieved.EndDate);
        Assert.Equal(MembershipStatus.Active, retrieved.Status);
        Assert.Equal("txn_persist_999", retrieved.PaymentTransactionId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMembershipDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
