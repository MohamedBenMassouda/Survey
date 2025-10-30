using System.Runtime.Serialization;

namespace Survey.Infrastructure.Enums;

public enum SurveyStatus
{
    [EnumMember(Value = "draft")]
    Draft,
    [EnumMember(Value = "published")]
    Published,
    [EnumMember(Value = "closed")]
    Closed
}