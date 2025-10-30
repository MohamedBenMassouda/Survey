namespace Survey.Infrastructure.DTO;

public class SubmitSurveyResponseRequest
{
    public string Token { get; set; } = string.Empty;
    public List<SubmitAnswerRequest> Answers { get; set; } = new();
}

public class SubmitAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string? AnswerText { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
}

public class SurveyResponseDto
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public string SurveyTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

