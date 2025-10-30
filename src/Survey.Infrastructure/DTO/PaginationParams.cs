using System.ComponentModel.DataAnnotations;
using Survey.Infrastructure.Enums;

namespace Survey.Infrastructure.DTO;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}

/// <summary>
/// Base class for query parameters with pagination, sorting, and filtering
/// </summary>
public class QueryParams : PaginationParams
{
    /// <summary>
    /// Sort field (e.g., "title", "createdAt")
    /// </summary>
    public string SortBy { get; set; }

    /// <summary>
    /// Sort direction: "asc" or "desc"
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Search query for text search
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Whether sort direction is ascending
    /// </summary>
    public bool IsAscending => SortDirection == SortDirection.Ascending;
}

/// <summary>
/// Query parameters for surveys
/// </summary>
public class SurveyQueryParams : QueryParams
{
    /// <summary>
    /// Filter by survey status (e.g., "Published", "Draft")
    /// </summary>
    public SurveyStatus? Status { get; set; }

    /// <summary>
    /// Filter by creator admin ID
    /// </summary>
    public Guid? CreatedByAdminId { get; set; }

    /// <summary>
    /// Filter by date range - start date
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Filter by date range - end date
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Include only active surveys
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Include only anonymous surveys
    /// </summary>
    public bool? AllowAnonymous { get; set; }
}

/// <summary>
/// Query parameters for questions
/// </summary>
public class QuestionQueryParams : QueryParams
{
    /// <summary>
    /// Filter by survey ID
    /// </summary>
    public Guid? SurveyId { get; set; }

    /// <summary>
    /// Filter by question type (e.g., "Rating", "Text")
    /// </summary>
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Filter by required flag
    /// </summary>
    public bool? IsRequired { get; set; }
}

/// <summary>
/// Query parameters for responses
/// </summary>
public class ResponseQueryParams : QueryParams
{
    /// <summary>
    /// Filter by survey ID
    /// </summary>
    public Guid? SurveyId { get; set; }

    /// <summary>
    /// Filter by completion status
    /// </summary>
    public bool? IsCompleted { get; set; }

    /// <summary>
    /// Filter by date range - started after
    /// </summary>
    public DateTime? StartedAfter { get; set; }

    /// <summary>
    /// Filter by date range - started before
    /// </summary>
    public DateTime? StartedBefore { get; set; }

    /// <summary>
    /// Filter by date range - completed after
    /// </summary>
    public DateTime? CompletedAfter { get; set; }

    /// <summary>
    /// Filter by date range - completed before
    /// </summary>
    public DateTime? CompletedBefore { get; set; }
}

/// <summary>
/// Query parameters for tokens
/// </summary>
public class TokenQueryParams : QueryParams
{
    /// <summary>
    /// Filter by survey ID
    /// </summary>
    public Guid? SurveyId { get; set; }

    /// <summary>
    /// Filter by used status
    /// </summary>
    public bool? IsUsed { get; set; }

    /// <summary>
    /// Include only expired tokens
    /// </summary>
    public bool? IsExpired { get; set; }

    /// <summary>
    /// Include only valid (not used, not expired) tokens
    /// </summary>
    public bool? IsValid { get; set; }
}

/// <summary>
/// Filter operator for building dynamic queries
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    In,
    NotIn
}

/// <summary>
/// Generic filter specification
/// </summary>
public class FilterSpec
{
    public string Field { get; set; }
    public FilterOperator Operator { get; set; }
    public object Value { get; set; }

    public FilterSpec(string field, FilterOperator op, object value)
    {
        Field = field;
        Operator = op;
        Value = value;
    }
}

/// <summary>
/// Sort specification
/// </summary>
public class SortSpec
{
    public string Field { get; set; }
    public bool IsAscending { get; set; }

    public SortSpec(string field, bool isAscending = true)
    {
        Field = field;
        IsAscending = isAscending;
    }
}