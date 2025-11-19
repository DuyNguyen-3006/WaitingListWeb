using Microsoft.AspNetCore.Http;
using Project.SharedLibrary.Logs;
using WaitingListWeb.Shared.ApiResponse;

namespace Project.SharedLibrary.MiddleWare
{
    public class GlobalException(RequestDelegate next)
    {
        private static readonly Dictionary<int, (string Title, string Message)> StatusMap = new()
        {
            { StatusCodes.Status400BadRequest, ("BadRequest", "Bad request. Please check your request and try again.") },
            { StatusCodes.Status401Unauthorized, ("Unauthorized", "Unauthorized access. Please login to continue.") },
            { StatusCodes.Status403Forbidden, ("Forbidden", "Forbidden access. You do not have permission.") },
            { StatusCodes.Status404NotFound, ("NotFound", "Resource not found. Please check the URL.") },
            { StatusCodes.Status429TooManyRequests, ("TooManyRequests", "Too many requests. Please try again later.") },
            { StatusCodes.Status500InternalServerError, ("InternalServerError", "Internal server error. Please try again later.") }
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);

                if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                {
                    var (title, message) = StatusMap.TryGetValue(context.Response.StatusCode, out var value)
                        ? value
                        : ("Error", "Unexpected error");

                    await ModifyHeader(context, context.Response.StatusCode, title, message);
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                if (ex is TimeoutException || ex is TaskCanceledException)
                {
                    await ModifyHeader(context, StatusCodes.Status408RequestTimeout, "RequestTimeout", "Request timeout. Please try again later.");
                }
                else
                {
                    await ModifyHeader(context, StatusCodes.Status500InternalServerError, "InternalServerError", ex.Message);
                }
            }
        }

        private static async Task ModifyHeader(HttpContext context, int statusCode, string title, string message)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>(statusCode, title, null, message);
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
