using System.ComponentModel.DataAnnotations;
using ASP_Chat.Controllers.Response;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
                if (!httpContext.Request.Path.ToString().StartsWith("/swagger") &&
                    httpContext.RequestServices.GetService(typeof(ModelStateDictionary)) is ModelStateDictionary modelState 
                    && !modelState.IsValid)
                {
                    var validationErrors = modelState
                        .Where(entry => entry.Value.Errors.Any())
                        .SelectMany(entry => entry.Value.Errors)
                        .Select(error => error.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Validation errors: {Errors}", string.Join(", ", validationErrors));

                    httpContext.Response.StatusCode = (int)StatusCodes.BadRequest;
                    await httpContext.Response.WriteAsync(new ApiResponse(
                        success: false,
                        errorCode: (int)ExceptionCodes.ValidationError,
                        errors: validationErrors
                    ).ToString());
                    return;
                }

                await _next(httpContext);
            }
            catch (ServerException ex)
            {
                _logger.LogError( ex, "Error: {Message}, StatusCode: {StatusCode}, Code: {Code}", 
                                    ex.Message, ex.StatusCode, ex.Code );
                httpContext.Response.StatusCode = (int)ex.StatusCode;
                await httpContext.Response.WriteAsync(new ApiResponse(
                        success: false,
                        errorCode: (int)ex.Code,
                        errors: new List<string> { ex.Message }
                    ).ToString());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error: {Message}, StatusCode: 500", ex.Message);
                httpContext.Response.StatusCode = (int)StatusCodes.InternalServerError;
                await httpContext.Response.WriteAsync(new ApiResponse(
                        success: false,
                        errorCode: (int)ExceptionCodes.InternalServerError,
                        errors: new List<string> { "Internal server error" }).ToString());
            }
        }
    }
}
