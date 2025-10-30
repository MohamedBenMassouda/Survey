using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Infrastructure.Models;

[Table("survey_response_answer_options")]
public class SurveyResponseAnswerOption : BaseEntity
{
    public Guid AnswerId { get; set; }
    public Guid QuestionOptionId { get; set; }

    public SurveyResponseAnswer? Answer { get; set; }
    public QuestionOption? QuestionOption { get; set; }
}