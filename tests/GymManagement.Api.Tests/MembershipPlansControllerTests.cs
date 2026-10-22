using System.Net;
using System.Net.Http.Json;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Api.Tests;

public class MembershipPlansControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAll_WhenNoPlansExist_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/membership-plans");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var plans = await response.Content.ReadFromJsonAsync<IReadOnlyList<MembershipPlanResponse>>();
        Assert.NotNull(plans);
        Assert.Empty(plans);
    }

    [Fact]
    public async Task Create_ValidPlan_ReturnsCreated_AndCanBeRetrievedThroughApiAndDatabase()
    {
        // Arrange
        var command = new CreateMembershipPlanCommand(
            Name: "Gold Annual",
            DurationInDays: 365,
            Price: 599.99m,
            MaxFreezeDays: 30,
            DisplayOrder: 1);

        // Act
        var response = await Client.PostAsJsonAsync("/api/membership-plans", command);

        // Assert HTTP response
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var createdPlan = await response.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        Assert.NotNull(createdPlan);
        Assert.NotEqual(Guid.Empty, createdPlan.Id);
        Assert.Equal(command.Name, createdPlan.Name);
        Assert.Equal(command.DurationInDays, createdPlan.DurationInDays);
        Assert.Equal(command.Price, createdPlan.Price);
        Assert.Equal(command.MaxFreezeDays, createdPlan.MaxFreezeDays);
        Assert.Equal(command.DisplayOrder, createdPlan.DisplayOrder);
        Assert.True(createdPlan.IsActive);

        // Verify retrieval through API (GetById via Location header)
        var getByIdResponse = await Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedPlan = await getByIdResponse.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        Assert.NotNull(retrievedPlan);
        Assert.Equal(createdPlan.Id, retrievedPlan.Id);
        Assert.Equal(command.Name, retrievedPlan.Name);
        Assert.Equal(command.DurationInDays, retrievedPlan.DurationInDays);
        Assert.Equal(command.Price, retrievedPlan.Price);
        Assert.True(retrievedPlan.IsActive);

        // Verify retrieval through API (GetAll)
        var getAllResponse = await Client.GetAsync("/api/membership-plans");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var allPlans = await getAllResponse.Content.ReadFromJsonAsync<IReadOnlyList<MembershipPlanResponse>>();
        Assert.NotNull(allPlans);
        Assert.Single(allPlans);
        Assert.Equal(createdPlan.Id, allPlans[0].Id);

        // Verify direct persistence in test database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var dbPlan = await dbContext.MembershipPlans.FindAsync(createdPlan.Id);
        Assert.NotNull(dbPlan);
        Assert.Equal(command.Name, dbPlan.Name);
        Assert.Equal(command.DurationInDays, dbPlan.DurationInDays);
        Assert.Equal(command.Price, dbPlan.Price);
        Assert.Equal(command.MaxFreezeDays, dbPlan.MaxFreezeDays);
        Assert.Equal(command.DisplayOrder, dbPlan.DisplayOrder);
        Assert.True(dbPlan.IsActive);
    }

    [Fact]
    public async Task Create_InvalidPlan_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange: Invalid command violating Name NotEmpty, DurationInDays allowed list, Price >= 0, etc.
        var invalidCommand = new CreateMembershipPlanCommand(
            Name: string.Empty,
            DurationInDays: 45, // not in [1, 7, 14, 30, 90, 180, 365]
            Price: -10m,
            MaxFreezeDays: -1,
            DisplayOrder: -1);

        // Act
        var response = await Client.PostAsJsonAsync("/api/membership-plans", invalidCommand);

        // Assert HTTP response
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Validation Error", problemDetails.Title);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Detail);

        // Verify specific validation error messages from CreateMembershipPlanValidator
        Assert.True(problemDetails.Errors.ContainsKey("Name"));
        Assert.Contains("Name is required.", problemDetails.Errors["Name"]);

        Assert.True(problemDetails.Errors.ContainsKey("DurationInDays"));
        Assert.Contains("DurationInDays must be one of: 1, 7, 14, 30, 90, 180, 365.", problemDetails.Errors["DurationInDays"]);

        Assert.True(problemDetails.Errors.ContainsKey("Price"));
        Assert.Contains("Price must be greater than or equal to zero.", problemDetails.Errors["Price"]);

        Assert.True(problemDetails.Errors.ContainsKey("MaxFreezeDays"));
        Assert.Contains("MaxFreezeDays must be greater than or equal to zero.", problemDetails.Errors["MaxFreezeDays"]);

        Assert.True(problemDetails.Errors.ContainsKey("DisplayOrder"));
        Assert.Contains("DisplayOrder must be greater than or equal to zero.", problemDetails.Errors["DisplayOrder"]);

        // Verify no plan was written to the test database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var planCount = await dbContext.MembershipPlans.CountAsync();
        Assert.Equal(0, planCount);
    }

    [Fact]
    public async Task DeactivateAndActivate_ExistingPlan_UpdatesActiveStatusCorrectly()
    {
        // Arrange: Create an active plan first
        var command = new CreateMembershipPlanCommand(
            Name: "Monthly Fitness",
            DurationInDays: 30,
            Price: 40m,
            MaxFreezeDays: 7,
            DisplayOrder: 2);

        var createResponse = await Client.PostAsJsonAsync("/api/membership-plans", command);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPlan = await createResponse.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        Assert.NotNull(createdPlan);
        Assert.True(createdPlan.IsActive);

        // Act 1: Deactivate the plan
        var deactivateResponse = await Client.PatchAsync($"/api/membership-plans/{createdPlan.Id}/deactivate", null);

        // Assert 1: Deactivation result
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var getDeactivatedResponse = await Client.GetAsync($"/api/membership-plans/{createdPlan.Id}");
        Assert.Equal(HttpStatusCode.OK, getDeactivatedResponse.StatusCode);
        var deactivatedPlan = await getDeactivatedResponse.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        Assert.NotNull(deactivatedPlan);
        Assert.False(deactivatedPlan.IsActive);

        // Verify filtering with activeOnly=true returns empty
        var activeOnlyResponse = await Client.GetAsync("/api/membership-plans?activeOnly=true");
        var activePlans = await activeOnlyResponse.Content.ReadFromJsonAsync<IReadOnlyList<MembershipPlanResponse>>();
        Assert.NotNull(activePlans);
        Assert.Empty(activePlans);

        // Act 2: Reactivate the plan
        var activateResponse = await Client.PatchAsync($"/api/membership-plans/{createdPlan.Id}/activate", null);

        // Assert 2: Activation result
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var getActivatedResponse = await Client.GetAsync($"/api/membership-plans/{createdPlan.Id}");
        Assert.Equal(HttpStatusCode.OK, getActivatedResponse.StatusCode);
        var activatedPlan = await getActivatedResponse.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        Assert.NotNull(activatedPlan);
        Assert.True(activatedPlan.IsActive);

        // Verify filtering with activeOnly=true now contains the plan
        var activeOnlyResponse2 = await Client.GetAsync("/api/membership-plans?activeOnly=true");
        var activePlans2 = await activeOnlyResponse2.Content.ReadFromJsonAsync<IReadOnlyList<MembershipPlanResponse>>();
        Assert.NotNull(activePlans2);
        Assert.Single(activePlans2);
        Assert.Equal(createdPlan.Id, activePlans2[0].Id);
    }

    [Fact]
    public async Task Activate_NonExistentPlan_ReturnsNotFound()
    {
        // Act
        var response = await Client.PatchAsync($"/api/membership-plans/{Guid.NewGuid()}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public async Task GetById_NonExistentPlan_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/membership-plans/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
    }
}
