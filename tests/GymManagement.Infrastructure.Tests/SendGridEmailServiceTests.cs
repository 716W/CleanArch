using System.Net;
using GymManagement.Infrastructure.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;

namespace GymManagement.Infrastructure.Tests;

public class SendGridEmailServiceTests
{
    private readonly Mock<ISendGridClient> _mockSendGridClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<SendGridEmailService>> _mockLogger;
    private readonly SendGridEmailService _sut;

    private const string DefaultFromEmail = "noreply@gymmanagement.com";
    private const string DefaultFromName = "Gym Management";

    public SendGridEmailServiceTests()
    {
        _mockSendGridClient = new Mock<ISendGridClient>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<SendGridEmailService>>();

        _mockConfiguration.Setup(c => c["Email:FromEmail"]).Returns(DefaultFromEmail);
        _mockConfiguration.Setup(c => c["Email:FromName"]).Returns(DefaultFromName);

        _sut = new SendGridEmailService(
            _mockSendGridClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    private static Response CreateSuccessResponse(HttpStatusCode statusCode = HttpStatusCode.Accepted)
    {
        return new Response(statusCode, new StringContent(string.Empty), null);
    }

    private static Response CreateFailureResponse(HttpStatusCode statusCode, string errorBody)
    {
        return new Response(statusCode, new StringContent(errorBody), null);
    }

    [Fact]
    public async Task SendAsync_WhenSendGridReturnsAccepted_CompletesSuccessfully()
    {
        // Arrange
        var to = "member@example.com";
        var subject = "Welcome to GymManagement";
        var body = "Thank you for joining our gym!";

        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResponse(HttpStatusCode.Accepted));

        // Act
        var exception = await Record.ExceptionAsync(() => _sut.SendAsync(to, subject, body));

        // Assert
        Assert.Null(exception);
        _mockSendGridClient.Verify(
            c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task SendAsync_WhenSendGridReturnsAnySuccessStatusCode_CompletesSuccessfully(HttpStatusCode statusCode)
    {
        // Arrange
        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResponse(statusCode));

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _sut.SendAsync("member@example.com", "Subject", "Body"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendAsync_WhenSendGridReturnsNonSuccessStatusCode_ThrowsInvalidOperationExceptionWithExactDetails()
    {
        // Arrange
        var to = "invalid@example.com";
        var subject = "Failure Test";
        var body = "Body Content";
        var statusCode = HttpStatusCode.BadRequest;
        var errorBody = "{\"errors\":[{\"message\":\"The from email does not match an active domain.\"}]}";

        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFailureResponse(statusCode, errorBody));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SendAsync(to, subject, body));

        var expectedMessage = $"Failed to send email via SendGrid. StatusCode: {statusCode}, Details: {errorBody}";
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Bad Request: malformed JSON payload")]
    [InlineData(HttpStatusCode.Unauthorized, "Unauthorized: invalid API key")]
    [InlineData(HttpStatusCode.Forbidden, "Forbidden: sender identity unverified")]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error")]
    public async Task SendAsync_WhenSendGridReturnsVariousNonSuccessStatusCodes_ThrowsInvalidOperationException(
        HttpStatusCode statusCode,
        string errorDetails)
    {
        // Arrange
        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateFailureResponse(statusCode, errorDetails));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SendAsync("member@example.com", "Subject", "Body"));

        Assert.Contains(statusCode.ToString(), exception.Message);
        Assert.Contains(errorDetails, exception.Message);
    }

    [Fact]
    public async Task SendAsync_ShouldConstructMessageWithConfiguredSenderEmailAndName()
    {
        // Arrange
        const string customFromEmail = "notifications@customgym.com";
        const string customFromName = "Custom Gym Notifications";

        _mockConfiguration.Setup(c => c["Email:FromEmail"]).Returns(customFromEmail);
        _mockConfiguration.Setup(c => c["Email:FromName"]).Returns(customFromName);

        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, _) => capturedMessage = msg)
            .ReturnsAsync(CreateSuccessResponse());

        // Act
        await _sut.SendAsync("member@example.com", "Test Subject", "Test Body");

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.NotNull(capturedMessage.From);
        Assert.Equal(customFromEmail, capturedMessage.From.Email);
        Assert.Equal(customFromName, capturedMessage.From.Name);
    }

    [Fact]
    public async Task SendAsync_ShouldConstructMessageWithRecipientSubjectAndBodyMappedCorrectly()
    {
        // Arrange
        const string to = "johndoe@example.com";
        const string subject = "Your Gym Membership has been activated";
        const string body = "<p>Welcome John! Your membership is now active.</p>";

        SendGridMessage? capturedMessage = null;
        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, _) => capturedMessage = msg)
            .ReturnsAsync(CreateSuccessResponse());

        // Act
        await _sut.SendAsync(to, subject, body);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.NotNull(capturedMessage.Personalizations);
        var personalization = Assert.Single(capturedMessage.Personalizations);
        Assert.Equal(subject, personalization.Subject);
        Assert.NotNull(personalization.Tos);
        var recipient = Assert.Single(personalization.Tos);
        Assert.Equal(to, recipient.Email);

        Assert.NotNull(capturedMessage.Contents);
        Assert.Contains(capturedMessage.Contents, c => c.Type == "text/plain" && c.Value == body);
        Assert.Contains(capturedMessage.Contents, c => c.Type == "text/html" && c.Value == body);
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_PassesCancellationTokenToSendGridClient()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), cancellationToken))
            .ReturnsAsync(CreateSuccessResponse());

        // Act
        await _sut.SendAsync("member@example.com", "Subject", "Body", cancellationToken);

        // Assert
        _mockSendGridClient.Verify(
            c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithoutCancellationToken_PassesDefaultCancellationTokenToSendGridClient()
    {
        // Arrange
        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResponse());

        // Act
        await _sut.SendAsync("member@example.com", "Subject", "Body");

        // Assert
        _mockSendGridClient.Verify(
            c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenCancellationIsRequested_PropagatesOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.SendAsync("member@example.com", "Subject", "Body", cancellationToken));
    }
}
