using GymManagement.Domain.Entities;
using GymManagement.Domain.Enums;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Tests;

public abstract class InfrastructureTestBase : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    protected GymDbContext Context { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new GymDbContext(options);
        await Context.Database.MigrateAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    protected GymDbContext CreateCleanContext()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new GymDbContext(options);
    }

    protected static Member CreateMember(
        string email = "member@example.com",
        string firstName = "John",
        string lastName = "Doe",
        DateTime? dateOfBirth = null,
        bool isActive = true)
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = dateOfBirth ?? new DateTime(1990, 1, 1),
            IsActive = isActive,
            JoinedDate = DateTime.UtcNow
        };
    }

    protected static MembershipPlan CreatePlan(
        string name = "Standard Plan",
        int durationInDays = 30,
        decimal price = 49.99m,
        int displayOrder = 1,
        bool isActive = true,
        int maxFreezeDays = 5)
    {
        return new MembershipPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            DurationInDays = durationInDays,
            Price = price,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            MaxFreezeDays = maxFreezeDays
        };
    }

    protected static Membership CreateMembership(
        Guid memberId,
        Guid membershipPlanId,
        DateTime startDate,
        DateTime endDate,
        MembershipStatus status = MembershipStatus.Active,
        string paymentTransactionId = "txn_123456")
    {
        return new Membership
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MembershipPlanId = membershipPlanId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            PaymentTransactionId = paymentTransactionId
        };
    }
}
