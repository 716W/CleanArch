using System.Net;
using System.Net.Http.Json;
using GymManagement.Application.Features.Members.Commands.CreateMember;
using GymManagement.Application.Features.Members.Commands.UpdateMember;
using GymManagement.Application.Features.Members.DTOs;
using GymManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Api.Tests;

public class MemberControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAll_WhenNoMembersExist_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/members");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var members = await response.Content.ReadFromJsonAsync<IReadOnlyList<MemberResponse>>();
        Assert.NotNull(members);
        Assert.Empty(members);
    }

    [Fact]
    public async Task Create_ValidMember_ReturnsCreated_AndCanBeRetrievedThroughApiAndDatabase()
    {
        // Arrange
        var command = new CreateMemberCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1995, 5, 20));

        // Act
        var response = await Client.PostAsJsonAsync("/api/members", command);

        // Assert HTTP response
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var createdMember = await response.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(createdMember);
        Assert.NotEqual(Guid.Empty, createdMember.Id);
        Assert.Equal(command.FirstName, createdMember.FirstName);
        Assert.Equal(command.LastName, createdMember.LastName);
        Assert.Equal(command.Email, createdMember.Email);
        Assert.Equal(command.DateOfBirth, createdMember.DateOfBirth);
        Assert.True(createdMember.IsActive);

        // Verify retrieval through API (GetById via Location header)
        var getByIdResponse = await Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var retrievedMember = await getByIdResponse.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(retrievedMember);
        Assert.Equal(createdMember.Id, retrievedMember.Id);
        Assert.Equal(command.FirstName, retrievedMember.FirstName);
        Assert.Equal(command.LastName, retrievedMember.LastName);
        Assert.Equal(command.Email, retrievedMember.Email);

        // Verify retrieval through API (GetAll)
        var getAllResponse = await Client.GetAsync("/api/members");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var allMembers = await getAllResponse.Content.ReadFromJsonAsync<IReadOnlyList<MemberResponse>>();
        Assert.NotNull(allMembers);
        Assert.Single(allMembers);
        Assert.Equal(createdMember.Id, allMembers[0].Id);

        // Verify direct persistence in test database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var dbMember = await dbContext.Members.FindAsync(createdMember.Id);
        Assert.NotNull(dbMember);
        Assert.Equal(command.FirstName, dbMember.FirstName);
        Assert.Equal(command.LastName, dbMember.LastName);
        Assert.Equal(command.Email, dbMember.Email);
        Assert.Equal(command.DateOfBirth, dbMember.DateOfBirth);
        Assert.True(dbMember.IsActive);
    }

    [Fact]
    public async Task Create_InvalidMember_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange: Invalid command violating FirstName NotEmpty, Email format, and DateOfBirth in the past
        var invalidCommand = new CreateMemberCommand(
            FirstName: string.Empty,
            LastName: "Doe",
            Email: "not-an-email",
            DateOfBirth: DateTime.Today.AddDays(1));

        // Act
        var response = await Client.PostAsJsonAsync("/api/members", invalidCommand);

        // Assert HTTP response
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        Assert.Equal("Validation Error", problemDetails.Title);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Detail);

        // Verify specific validation error messages from CreateMemberValidator
        Assert.True(problemDetails.Errors.ContainsKey("FirstName"));
        Assert.Contains("First name is required.", problemDetails.Errors["FirstName"]);

        Assert.True(problemDetails.Errors.ContainsKey("Email"));
        Assert.Contains("A valid email address is required.", problemDetails.Errors["Email"]);

        Assert.True(problemDetails.Errors.ContainsKey("DateOfBirth"));
        Assert.Contains("Date of birth must be in the past.", problemDetails.Errors["DateOfBirth"]);

        // Verify no member was written to the test database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var memberCount = await dbContext.Members.CountAsync();
        Assert.Equal(0, memberCount);
    }

    [Fact]
    public async Task Create_DuplicateEmail_ReturnsConflictWithProblemDetails_AndDoesNotModifyDatabase()
    {
        // Arrange
        var initialCommand = new CreateMemberCommand(
            "Original",
            "User",
            "duplicate.test@example.com",
            new DateTime(1990, 1, 1));

        var initialResponse = await Client.PostAsJsonAsync("/api/members", initialCommand);
        Assert.Equal(HttpStatusCode.Created, initialResponse.StatusCode);

        var duplicateCommand = new CreateMemberCommand(
            "Duplicate",
            "User",
            "duplicate.test@example.com",
            new DateTime(1992, 2, 2));

        // Act
        var conflictResponse = await Client.PostAsJsonAsync("/api/members", duplicateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("application/json", conflictResponse.Content.Headers.ContentType?.MediaType);

        var problemDetails = await conflictResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Contains("duplicate.test@example.com", problemDetails.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));

        // Verify database records remained unchanged
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var matchingMembers = await dbContext.Members
            .Where(m => m.Email == "duplicate.test@example.com")
            .ToListAsync();

        Assert.Single(matchingMembers);
        Assert.Equal("Original", matchingMembers[0].FirstName);
    }

    [Fact]
    public async Task Update_DuplicateEmailBelongingToAnotherMember_ReturnsConflictWithProblemDetails_AndDoesNotModifyDatabase()
    {
        // Arrange: Create Member 1 and Member 2
        var member1Command = new CreateMemberCommand("User1", "One", "user1@example.com", new DateTime(1990, 1, 1));
        var member2Command = new CreateMemberCommand("User2", "Two", "user2@example.com", new DateTime(1991, 1, 1));

        var res1 = await Client.PostAsJsonAsync("/api/members", member1Command);
        var res2 = await Client.PostAsJsonAsync("/api/members", member2Command);

        var member2 = await res2.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(member2);

        // Act: Attempt to update Member 2 to have Member 1's email
        var updateCommand = new UpdateMemberCommand(
            member2.Id,
            "User2Updated",
            "Two",
            "user1@example.com",
            new DateTime(1991, 1, 1));

        var updateResponse = await Client.PutAsJsonAsync($"/api/members/{member2.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
        Assert.Equal("application/json", updateResponse.Content.Headers.ContentType?.MediaType);

        var problemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Contains("user1@example.com", problemDetails.Detail);

        // Verify database: Member 2 retained original email and names
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var dbMember2 = await dbContext.Members.FindAsync(member2.Id);
        Assert.NotNull(dbMember2);
        Assert.Equal("user2@example.com", dbMember2.Email);
        Assert.Equal("User2", dbMember2.FirstName);
    }

    [Fact]
    public async Task Update_RetainingOwnEmail_ReturnsOkWithUpdatedMember()
    {
        // Arrange: Create Member
        var createCommand = new CreateMemberCommand("Retain", "Original", "retain@example.com", new DateTime(1993, 3, 3));
        var createResponse = await Client.PostAsJsonAsync("/api/members", createCommand);
        var member = await createResponse.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(member);

        // Act: Update member with new name but same email
        var updateCommand = new UpdateMemberCommand(
            member.Id,
            "RetainUpdated",
            "NewLastName",
            "retain@example.com",
            new DateTime(1993, 3, 3));

        var updateResponse = await Client.PutAsJsonAsync($"/api/members/{member.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(updated);
        Assert.Equal("RetainUpdated", updated.FirstName);
        Assert.Equal("retain@example.com", updated.Email);

        // Verify database
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
        var dbMember = await dbContext.Members.FindAsync(member.Id);
        Assert.NotNull(dbMember);
        Assert.Equal("RetainUpdated", dbMember.FirstName);
        Assert.Equal("retain@example.com", dbMember.Email);
    }
}
