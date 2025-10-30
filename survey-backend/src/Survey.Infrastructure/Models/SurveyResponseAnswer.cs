using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Infrastructure.Models;

[Table("survey_response_answers")]
public class SurveyResponseAnswer : BaseEntity
{
    public Guid ResponseId { get; set; }
    public Guid QuestionId { get; set; }
    public string? AnswerText { get; set; }

    public SurveyResponse? Response { get; set; }
    public Question? Question { get; set; }
}