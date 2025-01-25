using ASP_Chat.Controllers.Response;
using Microsoft.AspNetCore.SignalR;

namespace ASP_Chat.Exceptions
{
    public class HubServerExceptionMiddleware : IHubFilter
    {
        private readonly ILogger<HubServerExceptionMiddleware> _logger;

        public HubServerExceptionMiddleware(ILogger<HubServerExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (ServerException ex)
            {
                _logger.LogError(ex, "Error in Hub method {MethodName}: {Message}, StatusCode: {StatusCode}, Code: {Code}", 
                                    invocationContext.HubMethodName, ex.Message, ex.StatusCode, ex.Code);
                var response = new ApiResponse(
                        success: false,
                        errorCode: (int)ex.Code,
                        errors: new List<string> { ex.Message }
                );
                await invocationContext.Hub.Clients.Caller.SendAsync("Error", response);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Hub method {MethodName}: {Message}", invocationContext.HubMethodName, ex.Message);
                var response = new ApiResponse(
                        success: false,
                        errorCode: (int)ExceptionCodes.InternalServerError,
                        errors: new List<string> { "Internal server error" });
                await invocationContext.Hub.Clients.Caller.SendAsync("Error", response);
                return null;
            }
        }
    }
}
