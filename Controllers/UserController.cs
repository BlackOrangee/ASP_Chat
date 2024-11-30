﻿using ASP_Chat.Controllers.Request;
using ASP_Chat.Controllers.Response;
using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_Chat.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UserController(ILogger<UserController> logger, IUserService userService, IJwtService jwtService)
        {
            _logger = logger;
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetUserById(long id)
        {
            _logger.LogInformation("Getting user with id: {Id}", id);
            return Ok( new ApiResponse(data: _userService.GetUserById(id).ToString()));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetUsersByUsername([FromQuery] string username)
        {
            _logger.LogInformation("Getting users with username: {Username}", username);
            return Ok(new ApiResponse(data: _userService.GetUsersByUsername(username)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult UpdateUser([FromBody] UserRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            request.UserId = _jwtService.GetUserIdFromToken(authorizationHeader);
            _logger.LogInformation("Updating user with id: {Id}", request.UserId);
            return Ok(new ApiResponse(data: _userService.UpdateUser(request).ToString()));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult DeleteUser([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long id = _jwtService.GetUserIdFromToken(authorizationHeader);
            _logger.LogInformation("Deleting user with id: {Id}", id);
            return Ok(new ApiResponse(message: _userService.DeleteUser(id).ToString()));
        }
    }
}