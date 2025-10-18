namespace Template.API.Models.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
        public List<string>? Errors { get; init; }

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(T? data, string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Data = data, Errors = errors };

    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string? message = null) =>
            new() { Success = true, Message = message };

        public static ApiResponse Fail(string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
