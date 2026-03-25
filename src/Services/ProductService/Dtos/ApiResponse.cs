namespace ProductService.Dtos;

/// <summary>
/// Standard API Response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Data type returned by the API</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T? data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create a failure response
    /// </summary>
    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Generic response wrapper for lists
/// </summary>
public class ApiListResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new List<T>();
    public int Total { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiListResponse<T> SuccessResponse(List<T> data, int total = 0, string message = "Retrieved successfully")
    {
        return new ApiListResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Total = total
        };
    }

    public static ApiListResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new ApiListResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
