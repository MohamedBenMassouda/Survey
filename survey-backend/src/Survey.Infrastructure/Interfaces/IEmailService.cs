namespace Survey.Infrastructure.Interfaces;

public interface IEmailService
{
    Task SendSurveyInvitationAsync(string recipientEmail, string surveyTitle, string surveyLink, string token);
    Task SendBulkInvitationsAsync(List<string> recipientEmails, string surveyTitle, string surveyLink, Dictionary<string, string> emailTokenMap);
}