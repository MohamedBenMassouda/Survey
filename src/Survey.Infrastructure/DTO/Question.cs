using System.Text.Json.Serialization;
using Survey.Infrastructure.Enums;

namespace Survey.Infrastructure.DTO;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType QuestionType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateQuestionRequest
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public bool IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
    public List<CreateOptionRequest> Options { get; set; }
}

public class CreateOptionRequest
{
    public string OptionText { get; set; }
    public int? DisplayOrder { get; set; }
}