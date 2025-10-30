using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Survey.Infrastructure.Enums;

namespace Survey.Infrastructure.Models;

[Index(nameof(Title))]
[Index(nameof(IsActive), nameof(Status))]
[Table("surveys")]
public class SurveyModel : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }
    [MaxLength(2000)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SurveyStatus Status { get; set; } = SurveyStatus.Draft;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid CreatorId { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<SurveyToken> Tokens { get; set; } = new List<SurveyToken>();
    public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
    public Admin Creator { get; set; }
}