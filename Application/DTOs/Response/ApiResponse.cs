namespace Application.DTOs.Response;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<string>? Errors { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Parameterless
    public ApiResponse() { }

    // Success with data
    public ApiResponse(T data, string message = "Success", int statusCode = 200)
    {
        IsSuccess = true;
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    // Message only (success or failure)
    public ApiResponse(string message, bool isSuccess, int statusCode)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Errors = isSuccess ? null : new List<string> { message };
    }

    // Error list
    public ApiResponse(List<string> errors, string message = "Failed", int statusCode = 400)
    {
        IsSuccess = false;
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }
}