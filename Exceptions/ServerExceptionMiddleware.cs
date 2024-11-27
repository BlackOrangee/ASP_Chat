using ASP_Chat.Controllers.Response;

namespace ASP_Chat.Exceptions
{
    public class ServerExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ServerExceptionMiddleware> _logger;

        public ServerExceptionMiddleware(RequestDelegate next, ILogger<ServerExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ServerException ex)
            {
                _logger.LogError($"Error: {ex.Message}, StatusCode: {ex.StatusCode}, Code: {ex.Code}");
                httpContext.Response.StatusCode = (int)ex.StatusCode;
                await httpContext.Response.WriteAsync(new ApiResponse(
                        success: false,
                        errorCode: (int)ex.Code,
                        error: ex.Message
                    ).ToString());
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}, StatusCode: 500");
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
