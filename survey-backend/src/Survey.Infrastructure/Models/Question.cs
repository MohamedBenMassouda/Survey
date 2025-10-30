using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Survey.Infrastructure.Enums;

namespace Survey.Infrastructure.Models;

[Index(nameof(Type))]
[Index(nameof(DisplayOrder))]
[Table("questions")]
public class Question : BaseEntity
{
    public Guid SurveyId { get; set; }
    public required string QuestionText { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public SurveyModel? Survey { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}