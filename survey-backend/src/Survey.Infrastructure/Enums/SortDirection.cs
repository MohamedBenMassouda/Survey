using System.Runtime.Serialization;

namespace Survey.Infrastructure.Enums;

public enum SortDirection
{
    [EnumMember(Value = "asc")]
    Ascending,
    [EnumMember(Value = "desc")]
    Descending
}