using Microsoft.AspNetCore.Http;
namespace Project.SharedLibrary.MiddleWare
{
    public class ListenToOnlyApiGateWay(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var signedHeader = context.Request.Headers["Api-Gateway"];

            // Null means, the request is not from API Gateway
            if (string.IsNullOrEmpty(signedHeader))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable");
                return;
            }
            else
            {
                await next(context);
            }
        }
    }
}
