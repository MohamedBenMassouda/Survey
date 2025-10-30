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
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid CreatedBy { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}