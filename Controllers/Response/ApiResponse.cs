using Newtonsoft.Json;

namespace ASP_Chat.Controllers.Response
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
        public int? ErrorCode { get; set; }
        public string? Error { get; set; }

        public ApiResponse(bool success = true, object? data = null,
            string? message = null, int? errorCode = null, string? error = null)
        {
            Success = success;
            Data = data;
            Message = message;
            ErrorCode = errorCode;
            Error = error;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
