using System.Runtime.Serialization;

namespace Survey.Infrastructure.Enums;

public enum QuestionType
{
    [EnumMember(Value = "multiple_choice")]
    MultipleChoice,
    [EnumMember(Value = "text")]
    Text,
    [EnumMember(Value = "checkbox")]
    Checkbox,
    [EnumMember(Value = "rating")]
    Rating,
    [EnumMember(Value = "yes_no")]
    YesNo,
    [EnumMember(Value = "dropdown")]
    Dropdown,
    [EnumMember(Value = "scale")]
    Scale
}