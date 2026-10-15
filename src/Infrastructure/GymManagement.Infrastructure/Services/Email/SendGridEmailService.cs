namespace GymManagement.Infrastructure.Services.Email;

using GymManagement.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

public sealed class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        ISendGridClient client,
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];
        var from = new EmailAddress(fromEmail, fromName);

        var recipient = new EmailAddress(to);
        var message = MailHelper.CreateSingleEmail(
            from,
            recipient,
            subject,
            plainTextContent: body,
            htmlContent: body
        );

        _logger.LogInformation("Sending email to {Recipient} with subject {Subject}", to, subject);

        var response = await _client.SendEmailAsync(
            message,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to send email to {Recipient}. StatusCode: {StatusCode}, Details: {Details}",
                to,
                response.StatusCode,
                errorBody
            );

            throw new InvalidOperationException(
                $"Failed to send email via SendGrid. StatusCode: {response.StatusCode}, Details: {errorBody}");
        }

        _logger.LogInformation("Email successfully sent to {Recipient}", to);
    }
}