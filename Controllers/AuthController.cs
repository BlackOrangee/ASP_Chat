﻿using ASP_Chat.Service;
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
        public IActionResult Register([FromBody] AuthRequest request)
        {
            request.RegisterValidate();
            _logger.LogInformation("Username: {Username}. Try to register.", request.Username);
            return Ok(new ApiResponse(message: _authService.Register(request)));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthRequest request)
        {
            request.LoginValidate();
            _logger.LogInformation("Username: {Username}. Try to login.", request.Username);
            return Ok(new ApiResponse(data: new { token = _authService.Login(request) }));
        }

        [Authorize]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] AuthRequest request, 
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            request.ChangePasswordValidate();
            request.Id = _jwtService.GetUserIdFromToken(authorizationHeader);
            _logger.LogInformation("UserId: {Id}. Try to change password.", request.Id);

            return Ok(new ApiResponse( message: _authService.ChangePassword(request)));
        }
    }
}
