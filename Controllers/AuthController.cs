using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP_Chat.Controllers.Response;
using ASP_Chat.Service.Impl;



namespace ASP_Chat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] dynamic body)
        {
            _logger.LogInformation($"Username: {body.username}. Try to register.");
            return Ok(new ApiResponse(message: _authService.Register(body.username, body.password, body.name)));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] dynamic body)
        {
            _logger.LogInformation($"Username: {body.username}. Try to login.");
            return Ok(new ApiResponse(data: new { token = _authService.Login(body.username, body.password) }));
        }

        [Authorize]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] dynamic body)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]);
            _logger.LogInformation($"UserId: {userId}. Try to change password.");

            return Ok(new ApiResponse( message: _authService.ChangePassword(userId, body.oldPassword, body.newPassword)));
        }
    }
}
