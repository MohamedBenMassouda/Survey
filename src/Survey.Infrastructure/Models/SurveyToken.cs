using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Survey.Infrastructure.Models;

[Index(nameof(Token), IsUnique = true)]
[Index(nameof(IsUsed))]
[Table("survey_tokens")]
public class SurveyToken : BaseEntity
{
    public Guid SurveyId { get; set; }
    public required string Token { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    public SurveyModel? Survey { get; set; }
}