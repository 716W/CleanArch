using System.Net;
using System.Net.Http.Json;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.DTOs;
using GymManagement.Application.Features.Memberships.Commands.CreateMemberships;
using GymManagement.Application.Features.Memberships.DTOs;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Api.Tests;

public class MembershipsControllerTests : IntegrationTestBase
{
    private async Task<MemberResponse> CreateMemberAsync(string email = "member@example.com")
    {
        var command = new CreateMemberCommand("Test", "Member", email, new DateTime(1995, 1, 1));
        var response = await Client.PostAsJsonAsync("/api/members", command);
        response.EnsureSuccessStatusCode();
        var member = await response.Content.ReadFromJsonAsync<MemberResponse>();
        return member!;
    }

    private async Task<MembershipPlanResponse> CreatePlanAsync(string name = "Standard Monthly", int durationInDays = 30)
    {
        var command = new CreateMembershipPlanCommand(name, durationInDays, 49.99m, 5, 1);
        var response = await Client.PostAsJsonAsync("/api/membership-plans", command);
        response.EnsureSuccessStatusCode();
        var plan = await response.Content.ReadFromJsonAsync<MembershipPlanResponse>();
        return plan!;
    }

    [Fact]
    public async Task GetByMemberId_WhenMemberHasNoMemberships_ReturnsOkWithEmptyList()
    {
        // Arrange: Create a member with no memberships
        var member = await CreateMemberAsync();

        // Act
        var response = await Client.GetAsync($"/api/memberships/member/{member.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var memberships = await response.Content.ReadFromJsonAsync<IReadOnlyList<MembershipResponse>>();
        Assert.NotNull(memberships);
        Assert.Empty(memberships);
    }

    [Fact]
    public async Task GetByMemberId_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        // Act: Non-existent member ID
        var response = await Client.GetAsync($"/api/memberships/member/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public async Task Create_ValidMembership_ReturnsCreated_AndCanBeRetrievedThroughApiAndDatabase()
    {
        // Arrange: Create prerequisite member and plan
        var member = await CreateMemberAsync();
        var plan = await CreatePlanAsync(durationInDays: 30);

        var command = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-123456"
        };

        // Act: Create membership
        var response = await Client.PostAsJsonAsync("/api/memberships", command);

        // Assert HTTP response
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var createdMembership = await response.Content.ReadFromJsonAsync<MembershipResponse>();
        Assert.NotNull(createdMembership);
        Assert.NotEqual(Guid.Empty, createdMembership.Id);
        Assert.Equal(member.Id, createdMembership.MemberId);
        Assert.Equal(plan.Id, createdMembership.MembershipPlanId);
        Assert.Equal(command.StartDate.Date, createdMembership.StartDate);
        Assert.Equal(command.StartDate.Date.AddDays(plan.DurationInDays), createdMembership.EndDate);
        Assert.Equal("Active", createdMembership.Status);

        // Verify retrieval via Location header (GetById)
        var getByIdResponse = await Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedMembership = await getByIdResponse.Content.ReadFromJsonAsync<MembershipResponse>();
        Assert.NotNull(retrievedMembership);
        Assert.Equal(createdMembership.Id, retrievedMembership.Id);
        Assert.Equal(member.Id, retrievedMembership.MemberId);
        Assert.Equal(plan.Id, retrievedMembership.MembershipPlanId);
        Assert.Equal(createdMembership.StartDate, retrievedMembership.StartDate);
        Assert.Equal(createdMembership.EndDate, retrievedMembership.EndDate);
        Assert.Equal("Active", retrievedMembership.Status);

        // Verify retrieval via member memberships list
        var getByMemberResponse = await Client.GetAsync($"/api/memberships/member/{member.Id}");
        Assert.Equal(HttpStatusCode.OK, getByMemberResponse.StatusCode);
        var memberMemberships = await getByMemberResponse.Content.ReadFromJsonAsync<IReadOnlyList<MembershipResponse>>();
        Assert.NotNull(memberMemberships);
        Assert.Single(memberMemberships);
        Assert.Equal(createdMembership.Id, memberMemberships[0].Id);

        // Verify direct persistence in test database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var dbMembership = await dbContext.Memberships.FindAsync(createdMembership.Id);
        Assert.NotNull(dbMembership);
        Assert.Equal(member.Id, dbMembership.MemberId);
        Assert.Equal(plan.Id, dbMembership.MembershipPlanId);
        Assert.Equal(command.StartDate.Date, dbMembership.StartDate);
        Assert.Equal(command.StartDate.Date.AddDays(plan.DurationInDays), dbMembership.EndDate);
        Assert.Equal("TXN-123456", dbMembership.PaymentTransactionId);
    }

    [Fact]
    public async Task Create_InvalidMembershipInput_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange: Missing MemberId, PlanId, StartDate, and empty PaymentTransactionId
        var invalidCommand = new CreateMembershipCommand
        {
            MemberId = Guid.Empty,
            MembershipPlanId = Guid.Empty,
            StartDate = default,
            PaymentTransactionId = string.Empty
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/memberships", invalidCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Validation Error", problemDetails.Title);

        Assert.True(problemDetails.Errors.ContainsKey("MemberId"));
        Assert.True(problemDetails.Errors.ContainsKey("MembershipPlanId"));
        Assert.True(problemDetails.Errors.ContainsKey("StartDate"));
        Assert.True(problemDetails.Errors.ContainsKey("PaymentTransactionId"));

        // Verify no membership was written to database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var count = await dbContext.Memberships.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Create_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        // Arrange: Prerequisite plan exists, but member does not
        var plan = await CreatePlanAsync();

        var command = new CreateMembershipCommand
        {
            MemberId = Guid.NewGuid(),
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-NOTFOUND"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/memberships", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Contains(command.MemberId.ToString(), problemDetails.Detail);
    }

    [Fact]
    public async Task Create_WhenMembershipPlanDoesNotExist_ReturnsNotFound()
    {
        // Arrange: Prerequisite member exists, but plan does not
        var member = await CreateMemberAsync();

        var command = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-NOTFOUND"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/memberships", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Contains(command.MembershipPlanId.ToString(), problemDetails.Detail);
    }

    [Fact]
    public async Task Create_WhenMemberIsInactive_ReturnsConflict()
    {
        // Arrange: Create member and plan, then deactivate member in the test database
        var member = await CreateMemberAsync();
        var plan = await CreatePlanAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
            var dbMember = await dbContext.Members.FindAsync(member.Id);
            Assert.NotNull(dbMember);
            dbMember.IsActive = false;
            await dbContext.SaveChangesAsync();
        }

        var command = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-INACTIVE-MEMBER"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/memberships", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Business Rule Violation", problemDetails.Title);
        Assert.Contains("Member is inactive", problemDetails.Detail);
    }

    [Fact]
    public async Task Create_WhenMembershipPlanIsInactive_ReturnsConflict()
    {
        // Arrange: Create member and plan, then deactivate plan via API
        var member = await CreateMemberAsync();
        var plan = await CreatePlanAsync();

        var deactivateResponse = await Client.PatchAsync($"/api/membership-plans/{plan.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var command = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-INACTIVE-PLAN"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/memberships", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Business Rule Violation", problemDetails.Title);
        Assert.Contains("Membership plan is inactive", problemDetails.Detail);
    }

    [Fact]
    public async Task Create_WhenConflictingActiveMembershipExists_ReturnsConflict()
    {
        // Arrange: Create member and plan, and create first active membership
        var member = await CreateMemberAsync();
        var plan = await CreatePlanAsync(durationInDays: 30);

        var firstCommand = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date,
            PaymentTransactionId = "TXN-FIRST"
        };
        var firstResponse = await Client.PostAsJsonAsync("/api/memberships", firstCommand);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act: Attempt to create another membership that overlaps in date
        var conflictingCommand = new CreateMembershipCommand
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(5),
            PaymentTransactionId = "TXN-CONFLICTING"
        };
        var response = await Client.PostAsJsonAsync("/api/memberships", conflictingCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Contains("conflicting membership", problemDetails.Detail);
    }

    [Fact]
    public async Task GetById_WhenMembershipDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/memberships/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
    }
}
