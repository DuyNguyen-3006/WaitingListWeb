using System.Net;
using System.Text.Json.Serialization;

namespace WaitingListWeb.Shared.ApiResponse
{
    public class ApiResponse<T>
    {
        private string title;
        private object value;

        [JsonPropertyOrder(1)]
        public int StatusCode { get; init; }

        [JsonPropertyOrder(2)]
        public string Code { get; init; } = default!;

        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; init; }

        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        private ApiResponse(int statusCode, string code, string? message = null, T? data = default)
        {
            StatusCode = statusCode;
            Code = code;
            Message = message;
            Data = data;
        }

        public ApiResponse(int statusCode, string title, object value, string message)
        {
            StatusCode = statusCode;
            this.title = title;
            this.value = value;
            Message = message;
        }

        public static ApiResponse<T> Success(T data, string? message = null)
            => new((int)HttpStatusCode.OK, "SUCCESS", message, data);

        public static ApiResponse<T> Created(T data, string? message = null)
            => new((int)HttpStatusCode.Created, "CREATED", message, data);

        public static ApiResponse<T> Fail(HttpStatusCode status, string code, string? message = null)
            => new((int)status, code, message);

        public static ApiResponse<T> ValidationError(string message)
            => new((int)HttpStatusCode.BadRequest, "VALIDATION_ERROR", message);
    }
}
