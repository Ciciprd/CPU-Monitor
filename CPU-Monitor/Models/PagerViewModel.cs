namespace CPU_Monitor.Models;

/// <summary>
/// Represents pagination information for a collection of items.
/// </summary>
public class PagerViewModel
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Gets the maximum number of items displayed on each page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items in the collection.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Gets the total number of pages required to display all items.
    /// </summary>
    /// <remarks>
    /// Returns zero when <see cref="PageSize"/> is less than or equal to zero.
    /// </remarks>
    public int TotalPages => 
            PageSize > 0
                ? (int)Math.Ceiling((decimal)TotalItems / PageSize)
                : 0;

    /// <summary>
    /// Gets a value indicating whether a previous page is available.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Gets a value indicating whether a next page is available.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}
