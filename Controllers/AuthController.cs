using ASP_Chat.Controllers.Request;
using ASP_Chat.Controllers.Response;
using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_Chat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(ILogger<AuthController> logger,
                              IAuthService authService,
                              IJwtService jwtService)
        {
            _logger = logger;
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult Register([FromBody] AuthRegisterRequest request)
        {
            _logger.LogInformation("Username: {Username}. Try to register.", request.Username);
            return Ok(new ApiResponse(message: _authService.Register(request)));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult Login([FromBody] AuthLoginRequest request)
        {
            _logger.LogInformation("Username: {Username}. Try to login.", request.Username);
            return Ok(new ApiResponse(data: _authService.Login(request)));
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult ChangePassword([FromBody] AuthChangePasswordRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            _logger.LogInformation("UserId: {Id}. Try to change password.", userId);

            return Ok(new ApiResponse(message: _authService.ChangePassword(userId, request)));
        }
    }
}
