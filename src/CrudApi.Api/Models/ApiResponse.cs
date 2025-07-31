namespace CrudApi.Api.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the response data
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Gets or sets the list of errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the trace identifier for debugging
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates a successful API response
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="message">The success message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error API response
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errors">The list of errors</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates an error API response with a single error
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="error">The single error</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> ErrorResult(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}

/// <summary>
/// Standard API response wrapper without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful API response without data
    /// </summary>
    /// <param name="message">The success message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse SuccessResult(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates an error API response without data
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errors">The list of errors</param>
    /// <returns>An error API response</returns>
    public static new ApiResponse ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates an error API response without data with a single error
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="error">The single error</param>
    /// <returns>An error API response</returns>
    public static new ApiResponse ErrorResult(string message, string error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets the collection of items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    /// <summary>
    /// Gets or sets the current page number
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of records
    /// </summary>
    public int TotalRecords { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Gets a value indicating whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Query parameters for pagination
/// </summary>
public class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Gets or sets the page number (default is 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size (maximum is 100, default is 10)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

/// <summary>
/// Query parameters for sorting
/// </summary>
public class SortingParameters
{
    /// <summary>
    /// Gets or sets the field to sort by
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Gets or sets the sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Gets a value indicating whether the sort direction is descending
    /// </summary>
    public bool IsDescending => SortDirection.ToLower() == "desc";
}

/// <summary>
/// Combined query parameters for pagination, sorting, and searching
/// </summary>
public class QueryParameters : PaginationParameters
{
    /// <summary>
    /// Gets or sets the search term to filter results
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Gets or sets the field to sort by
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Gets or sets the sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Gets a value indicating whether the sort direction is descending
    /// </summary>
    public bool IsDescending => SortDirection.ToLower() == "desc";
}
