using System.Text.Json.Serialization;

namespace ASP_Chat.Controllers.Response
{
    public class ApiResponse
    {
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("Data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Data { get; set; }

        [JsonPropertyName("Message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("ErrorCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ErrorCode { get; set; }

        [JsonPropertyName("Errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }

        public ApiResponse(bool success = true, object? data = null,
            string? message = null, int? errorCode = null, List<string>? errors = null)
        {
            Success = success;
            Data = data;
            Message = message;
            ErrorCode = errorCode;
            Errors = errors;
        }

    }
}
