using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Survey.Infrastructure.Configuration;
using Survey.Infrastructure.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace Survey.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly TransactionalEmailsApi _apiInstance;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;

        // Configure Brevo API client
        brevo_csharp.Client.Configuration.Default.ApiKey.Add("api-key", _emailSettings.Password);
        _apiInstance = new TransactionalEmailsApi();
    }

    public async Task SendSurveyInvitationAsync(
        string recipientEmail,
        string surveyTitle,
        string surveyLink,
        string token)
    {
        try
        {
            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail),

                To = new List<SendSmtpEmailTo>
                {
                    new SendSmtpEmailTo(recipientEmail)
                },

                Subject = $"You're invited to participate: {surveyTitle}",

                HtmlContent = GenerateEmailHtml(surveyTitle, surveyLink, token)
            };

            var result = await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);

            _logger.LogInformation(
                "Survey invitation sent successfully to {Email}. MessageId: {MessageId}",
                recipientEmail,
                result.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send survey invitation to {Email}",
                recipientEmail);
            throw;
        }
    }

    public async Task SendBulkInvitationsAsync(
        List<string> recipientEmails,
        string surveyTitle,
        string surveyLink,
        Dictionary<string, string> emailTokenMap)
    {
        var tasks = recipientEmails.Select(async email =>
        {
            if (emailTokenMap.TryGetValue(email, out var token))
            {
                await SendSurveyInvitationAsync(email, surveyTitle, surveyLink, token);
            }
            else
            {
                _logger.LogWarning(
                    "No token found for email {Email}, skipping invitation",
                    email);
            }
        });

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation(
                "Bulk survey invitations completed. Total: {Count}",
                recipientEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred during bulk survey invitation sending");
            throw;
        }
    }

    private string GenerateEmailHtml(string surveyTitle, string surveyLink, string token)
    {
        var personalizedLink = $"{surveyLink}?token={token}";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 30px; 
            background-color: #4CAF50; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 20px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Survey Invitation</h1>
        </div>
        <div class='content'>
            <h2>{surveyTitle}</h2>
            <p>Hello,</p>
            <p>You have been invited to participate in our survey. Your feedback is valuable to us and will help improve our services.</p>
            <p>Click the button below to get started:</p>
            <div style='text-align: center;'>
                <a href='{personalizedLink}' class='button'>Take Survey</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; background-color: #eee; padding: 10px;'>{personalizedLink}</p>
            <p>Thank you for your time!</p>
        </div>
        <div class='footer'>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";
    }
}