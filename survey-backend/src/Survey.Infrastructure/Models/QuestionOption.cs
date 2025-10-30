using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Infrastructure.Models;

[Table("question_options")]
public class QuestionOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public required string OptionText { get; set; }
    public int DisplayOrder { get; set; }

    public Question? Question { get; set; }
}