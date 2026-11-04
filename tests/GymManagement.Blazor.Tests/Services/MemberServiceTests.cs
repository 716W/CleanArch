using GymManagement.Blazor.Contracts;
using GymManagement.Blazor.Generated;
using GymManagement.Blazor.Services.Base;
using GymManagement.Blazor.Services.Members;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Blazor.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IClient> _mockClient;
    private readonly Mock<ILogger<MemberService>> _mockLogger;
    private readonly MemberService _sut;

    public MemberServiceTests()
    {
        _mockClient = new Mock<IClient>();
        _mockLogger = new Mock<ILogger<MemberService>>();
        _sut = new MemberService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsReadOnlyListFromGeneratedClient()
    {
        // Arrange
        var members = new List<MemberResponse>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com", IsActive = true },
            new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", IsActive = true }
        };

        _mockClient
            .Setup(c => c.MemberAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("John", result[0].FirstName);
        Assert.Equal("Jane", result[1].FirstName);
        _mockClient.Verify(c => c.MemberAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberExists_ReturnsMemberResponse()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var expected = new MemberResponse
        {
            Id = memberId,
            FirstName = "Alice",
            LastName = "Walker",
            Email = "alice@example.com",
            IsActive = true
        };

        _mockClient
            .Setup(c => c.MemberGETAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetByIdAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal("Alice", result.FirstName);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ThrowsGymManagementApiExceptionWithErrors()
    {
        // Arrange
        var command = new CreateMemberCommand { FirstName = "", LastName = "", Email = "invalid" };
        var validationErrors = new Dictionary<string, ICollection<string>>
        {
            ["FirstName"] = new List<string> { "'First Name' must not be empty." },
            ["Email"] = new List<string> { "'Email' is not a valid email address." }
        };
        var problemDetails = new ValidationProblemDetails
        {
            Title = "One or more validation errors occurred.",
            Status = 400,
            Errors = validationErrors
        };

        var apiException = new ApiException<ValidationProblemDetails>(
            "Bad Request",
            400,
            "response body",
            new Dictionary<string, IEnumerable<string>>(),
            problemDetails,
            null!);

        _mockClient
            .Setup(c => c.MemberPOSTAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<GymManagementApiException>(() => _sut.CreateAsync(command));

        Assert.Equal(400, ex.StatusCode);
        Assert.True(ex.IsValidationError);
        Assert.NotNull(ex.ValidationErrors);
        Assert.True(ex.ValidationErrors.ContainsKey("FirstName"));
        Assert.True(ex.ValidationErrors.ContainsKey("Email"));
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsGymManagementApiExceptionWithIsNotFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var problemDetails = new ProblemDetails
        {
            Title = "Not Found",
            Detail = $"Member with id {memberId} was not found.",
            Status = 404
        };

        var apiException = new ApiException<ProblemDetails>(
            "Not Found",
            404,
            "response body",
            new Dictionary<string, IEnumerable<string>>(),
            problemDetails,
            null!);

        _mockClient
            .Setup(c => c.MemberGETAsync(memberId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<GymManagementApiException>(() => _sut.GetByIdAsync(memberId));

        Assert.Equal(404, ex.StatusCode);
        Assert.True(ex.IsNotFound);
        Assert.False(ex.IsValidationError);
        Assert.Equal("Not Found", ex.Title);
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_CompletesWithoutError()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        _mockClient
            .Setup(c => c.MemberDELETEAsync(memberId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(() => _sut.DeleteAsync(memberId));

        // Assert
        Assert.Null(exception);
        _mockClient.Verify(c => c.MemberDELETEAsync(memberId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

