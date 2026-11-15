using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using GymManagement.Application.Contracts.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Tests;

public class AuthControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsOkWithJwtToken()
    {
        // Arrange
        var request = new AuthRequest("admin@gymmanagement.com", "Admin@123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.Equal("admin@gymmanagement.com", result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);
        Assert.Equal("GymManagement", jwtToken.Issuer);
        Assert.Contains(jwtToken.Claims, c => (c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role) && c.Value == "Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsConflictProblemDetails()
    {
        // Arrange
        var request = new AuthRequest("admin@gymmanagement.com", "WrongPassword!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Invalid email or password", problem.Detail);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsConflictProblemDetails()
    {
        // Arrange
        var request = new AuthRequest("unknown@gymmanagement.com", "SomePassword123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Invalid email or password", problem.Detail);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOk_AndAllowsLogin()
    {
        // Arrange
        var registerRequest = new RegistrationRequest(
            FirstName: "New",
            LastName: "Member",
            UserName: "newmember",
            Email: "newmember@example.com",
            Password: "Password123!");

        // Act
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var regResult = await registerResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(regResult);
        Assert.False(string.IsNullOrWhiteSpace(regResult.UserId));

        // Now test login with newly registered credentials
        var loginRequest = new AuthRequest("newmember@example.com", "Password123!");
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginResult);
        Assert.Equal("newmember@example.com", loginResult.Email);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var request = new RegistrationRequest(
            FirstName: "Admin",
            LastName: "Duplicate",
            UserName: "uniqueusername",
            Email: "admin@gymmanagement.com",
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("already exists", problem.Detail);
    }

    [Fact]
    public async Task Register_WithExistingUserName_ReturnsConflict()
    {
        // Arrange
        var request = new RegistrationRequest(
            FirstName: "User",
            LastName: "Duplicate",
            UserName: "admin@gymmanagement.com",
            Email: "uniqueemail@example.com",
            Password: "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("already taken", problem.Detail);
    }
}
