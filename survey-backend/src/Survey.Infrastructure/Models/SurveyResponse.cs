using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Infrastructure.Models;

[Table("survey_responses")]
public class SurveyResponse : BaseEntity
{
    public Guid SurveyId { get; set; }
    public Guid TokenId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public SurveyModel? Survey { get; set; }
    public SurveyToken? Token { get; set; }
}