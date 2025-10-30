namespace Survey.Infrastructure.DTO;

public class SurveyDto : BaseDto
{
    public string Title { get; set; }
    public string Status { get; set; }
    public string CreatedByName { get; set; }
    public int QuestionCount { get; set; }
    public int ResponseCount { get; set; }
}

public class SurveyDetailDto : BaseDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public bool AllowAnonymous { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AdminDto CreatedBy { get; set; }
    public List<QuestionDto> Questions { get; set; }
}

public class SurveyAnalyticsDto
{
    public Guid SurveyId { get; set; }
    public string SurveyTitle { get; set; }
    public int TotalQuestions { get; set; }
    public int TokensGenerated { get; set; }
    public int TotalResponses { get; set; }
    public double ResponseRate { get; set; }
    public double AverageCompletionTime { get; set; }
}

public class CreateSurveyRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<CreateQuestionRequest> Questions { get; set; }
}

public class UpdateSurveyRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SendInvitationRequest
{
    public Guid SurveyId { get; set; }
    public List<string> RecipientEmails { get; set; } = new();
    public string? CustomMessage { get; set; }
}

public class InvitationResponse
{
    public int TotalInvitations { get; set; }
    public List<string> SuccessfulInvitations { get; set; } = new();
    public List<InvitationError> FailedInvitations { get; set; } = new();
}

public class InvitationError
{
    public string Email { get; set; }
    public string ErrorMessage { get; set; }
}
