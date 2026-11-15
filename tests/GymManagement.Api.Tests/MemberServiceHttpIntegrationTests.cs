using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Base;
using GymManagement.Blazor.Services.Members;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GymManagement.Api.Tests;

/// <summary>
/// End-to-end HTTP integration test verifying that the actual Blazor communication
/// layer (IMemberService -> MemberService -> Client -> HttpClient) successfully
/// communicates over HTTP with the ASP.NET Core Web API pipeline and SQLite database.
/// </summary>
public class MemberServiceHttpIntegrationTests : IntegrationTestBase
{
    private IMemberService CreateMemberService()
    {
        // Client from IntegrationTestBase is an HttpClient connected to the ASP.NET Core TestServer
        var apiClient = new Client(Client.BaseAddress?.ToString() ?? "http://localhost", Client);
        var logger = NullLogger<MemberService>.Instance;
        return new MemberService(apiClient, logger);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalledThroughRealHttpPipeline_ReturnsEmptyOrExistingListWithHttp200()
    {
        // Arrange
        var memberService = CreateMemberService();

        // Act - Sends actual HTTP GET to /api/members through NSwag client & ASP.NET Core pipeline
        var members = await memberService.GetAllAsync();

        // Assert
        Assert.NotNull(members);
    }

    [Fact]
    public async Task CreateAndGetById_WhenCalledThroughRealHttpPipeline_PersistsAndRetrievesMember()
    {
        // Arrange
        var memberService = CreateMemberService();
        var uniqueEmail = $"http-test-{Guid.NewGuid():N}@example.com";
        var createCommand = new CreateMemberCommand
        {
            FirstName = "HttpIntegration",
            LastName = "Tester",
            Email = uniqueEmail,
            DateOfBirth = new DateTimeOffset(new DateTime(1990, 5, 20), TimeSpan.Zero)
        };

        // Act 1: Send actual HTTP POST /api/members via IMemberService
        var created = await memberService.CreateAsync(createCommand);

        // Assert 1: HTTP 201 Created response received and deserialized
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("HttpIntegration", created.FirstName);
        Assert.Equal("Tester", created.LastName);
        Assert.Equal(uniqueEmail, created.Email);

        // Act 2: Send actual HTTP GET /api/members/{id} via IMemberService
        var retrieved = await memberService.GetByIdAsync(created.Id);

        // Assert 2: HTTP 200 OK received and verified against database
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(uniqueEmail, retrieved.Email);

        // Act 3: Verify it appears in HTTP GET /api/members
        var allMembers = await memberService.GetAllAsync();
        Assert.Contains(allMembers, m => m.Id == created.Id);

        // Act 4: Send actual HTTP DELETE /api/members/{id} via IMemberService
        await memberService.DeleteAsync(created.Id);

        // Act 5: Verify subsequent GET throws 404 through BaseApiService
        var ex = await Assert.ThrowsAsync<GymManagementApiException>(() => memberService.GetByIdAsync(created.Id));
        Assert.Equal(404, ex.StatusCode);
        Assert.True(ex.IsNotFound);
    }
}

