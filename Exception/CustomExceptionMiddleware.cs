namespace ASP_Chat.Exception
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
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
            catch (CustomException ex)
            {
                _logger.LogError($"Error: {ex.Message}, StatusCode: {ex.StatusCode}, Code: {ex.Code}");
                httpContext.Response.StatusCode = (int)ex.StatusCode;
                await httpContext.Response.WriteAsync(new CustomExceptionResponce((int)ex.Code, ex.Message).ToString());
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
