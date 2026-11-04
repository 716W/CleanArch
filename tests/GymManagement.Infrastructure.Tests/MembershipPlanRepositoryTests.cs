using GymManagement.Domain.Entities;
using GymManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Tests;

public class MembershipPlanRepositoryTests : InfrastructureTestBase
{
    private MembershipPlanRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new MembershipPlanRepository(Context);
    }

    [Fact]
    public async Task GetAllAsync_WhenActiveOnlyIsTrue_ShouldReturnOnlyActivePlans()
    {
        // Arrange
        var active1 = CreatePlan("Active Plan A", displayOrder: 1, isActive: true);
        var active2 = CreatePlan("Active Plan B", displayOrder: 2, isActive: true);
        var inactive1 = CreatePlan("Inactive Plan X", displayOrder: 3, isActive: false);
        var inactive2 = CreatePlan("Inactive Plan Y", displayOrder: 4, isActive: false);

        await Context.MembershipPlans.AddRangeAsync(active1, active2, inactive1, inactive2);
        await Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync(activeOnly: true);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.True(p.IsActive));
        Assert.Contains(results, p => p.Id == active1.Id);
        Assert.Contains(results, p => p.Id == active2.Id);
        Assert.DoesNotContain(results, p => p.Id == inactive1.Id);
        Assert.DoesNotContain(results, p => p.Id == inactive2.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenActiveOnlyIsFalse_ShouldReturnOnlyInactivePlans()
    {
        // Arrange
        var active1 = CreatePlan("Active Plan A", displayOrder: 1, isActive: true);
        var inactive1 = CreatePlan("Inactive Plan X", displayOrder: 2, isActive: false);
        var inactive2 = CreatePlan("Inactive Plan Y", displayOrder: 3, isActive: false);

        await Context.MembershipPlans.AddRangeAsync(active1, inactive1, inactive2);
        await Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync(activeOnly: false);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.False(p.IsActive));
        Assert.Contains(results, p => p.Id == inactive1.Id);
        Assert.Contains(results, p => p.Id == inactive2.Id);
        Assert.DoesNotContain(results, p => p.Id == active1.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilteringIsDisabled_ShouldReturnAllPlans()
    {
        // Arrange
        var active1 = CreatePlan("Active Plan A", displayOrder: 1, isActive: true);
        var inactive1 = CreatePlan("Inactive Plan X", displayOrder: 2, isActive: false);

        await Context.MembershipPlans.AddRangeAsync(active1, inactive1);
        await Context.SaveChangesAsync();

        // Act: test both explicit null and default parameter
        var resultsExplicitNull = await _repository.GetAllAsync(activeOnly: null);
        var resultsDefault = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, resultsExplicitNull.Count);
        Assert.Equal(2, resultsDefault.Count);
        Assert.Contains(resultsExplicitNull, p => p.Id == active1.Id);
        Assert.Contains(resultsExplicitNull, p => p.Id == inactive1.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByDisplayOrderAscending_ThenByNameAscending()
    {
        // Arrange:
        // Group with DisplayOrder 1: "Gold", "Bronze", "Diamond" -> Should order as Bronze, Diamond, Gold
        // Group with DisplayOrder 2: "Silver"
        // Group with DisplayOrder 3: "Platinum"
        var plan1 = CreatePlan(name: "Silver", displayOrder: 2);
        var plan2 = CreatePlan(name: "Gold", displayOrder: 1);
        var plan3 = CreatePlan(name: "Bronze", displayOrder: 1);
        var plan4 = CreatePlan(name: "Platinum", displayOrder: 3);
        var plan5 = CreatePlan(name: "Diamond", displayOrder: 1);

        await Context.MembershipPlans.AddRangeAsync(plan1, plan2, plan3, plan4, plan5);
        await Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        var namesInOrder = results.Select(p => p.Name).ToList();
        var expectedOrder = new List<string> { "Bronze", "Diamond", "Gold", "Silver", "Platinum" };

        Assert.Equal(expectedOrder, namesInOrder);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPlan_AndGetByIdAsync_ShouldRetrieveIt()
    {
        // Arrange
        var plan = CreatePlan(
            name: "Premium Annual",
            durationInDays: 365,
            price: 499.00m,
            displayOrder: 5,
            isActive: true,
            maxFreezeDays: 30);

        // Act
        var addedPlan = await _repository.AddAsync(plan);

        // Assert
        Assert.Equal(plan.Id, addedPlan.Id);

        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MembershipPlanRepository(cleanContext);
        var retrieved = await cleanRepo.GetByIdAsync(plan.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(plan.Id, retrieved.Id);
        Assert.Equal("Premium Annual", retrieved.Name);
        Assert.Equal(365, retrieved.DurationInDays);
        Assert.Equal(499.00m, retrieved.Price);
        Assert.Equal(5, retrieved.DisplayOrder);
        Assert.True(retrieved.IsActive);
        Assert.Equal(30, retrieved.MaxFreezeDays);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPlanDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePersistedPlan()
    {
        // Arrange
        var plan = CreatePlan("Initial Plan", price: 29.99m, displayOrder: 1, isActive: true);
        await Context.MembershipPlans.AddAsync(plan);
        await Context.SaveChangesAsync();

        // Act
        plan.Name = "Updated Plan Name";
        plan.Price = 39.99m;
        plan.DisplayOrder = 10;
        plan.IsActive = false;
        await _repository.UpdateAsync(plan);

        // Assert
        using var cleanContext = CreateCleanContext();
        var cleanRepo = new MembershipPlanRepository(cleanContext);
        var updated = await cleanRepo.GetByIdAsync(plan.Id);

        Assert.NotNull(updated);
        Assert.Equal("Updated Plan Name", updated.Name);
        Assert.Equal(39.99m, updated.Price);
        Assert.Equal(10, updated.DisplayOrder);
        Assert.False(updated.IsActive);
    }
}
