namespace UserService.API.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
           return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> FailResponse(string message, List<string>? errors = null, T? data = default)
        {
            return new ApiResponse<T> { Success = false, Data = data, Message = message, Errors = errors };
        }
    }
}
