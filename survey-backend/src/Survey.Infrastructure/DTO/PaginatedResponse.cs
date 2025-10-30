namespace Survey.Infrastructure.DTO;

public class PagedResult<T>(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = items;

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = pageNumber;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = pageSize;

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; } = totalCount;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Index of first item in current page (1-based)
    /// </summary>
    public int FirstItemOnPage => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;

    /// <summary>
    /// Index of last item in current page (1-based)
    /// </summary>
    public int LastItemOnPage => Math.Min(PageNumber * PageSize, TotalCount);

    public PagedResult() : this([], 0, 0, 0)
    {
    }

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new PagedResult<TResult>(
            Items.Select(mapper),
            TotalCount,
            PageNumber,
            PageSize
        );
    }
}