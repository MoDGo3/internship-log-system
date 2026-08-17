namespace InternLog.API.Core.Common;

/// <summary>
/// Response กลางสำหรับทุก API endpoint
/// รูปแบบ: { "success": bool, "message": string, "data": T }
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ApiResponse(bool success, string message, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    // Static helpers
    public static ApiResponse<T> Ok(T data, string message = "สำเร็จ")
        => new(true, message, data);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default);
}

/// <summary>
/// Response สำหรับ list พร้อม pagination metadata
/// </summary>
public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PagedApiResponse(IEnumerable<T> data, int totalCount, int page, int pageSize)
        : base(true, "สำเร็จ", data)
    {
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
