using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP_Chat.Controllers.Response;
using ASP_Chat.Service.Impl;
using ASP_Chat.Controllers.Request;



namespace ASP_Chat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, JwtService jwtService)
        {
            _logger = logger;
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthRequest user)
        {
            user.RegisterValidate();
            _logger.LogInformation($"Username: {user.Username}. Try to register.");
            return Ok(new ApiResponse(message: _authService.Register(user)));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthRequest user)
        {
            user.LoginValidate();
            _logger.LogInformation($"Username: {user.Username}. Try to login.");
            return Ok(new ApiResponse(data: new { token = _authService.Login(user) }));
        }

        [Authorize]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] AuthRequest user)
        {
            user.ChangePasswordValidate();
            user.Id = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            _logger.LogInformation($"UserId: {user.Id}. Try to change password.");

            return Ok(new ApiResponse( message: _authService.ChangePassword(user)));
        }
    }
}
