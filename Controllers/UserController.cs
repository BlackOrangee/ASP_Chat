using ASP_Chat.Controllers.Response;
using ASP_Chat.Service;
using ASP_Chat.Service.Impl;
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

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("{id}")] 
        public IActionResult GetUserById(long id)
        {
            _logger.LogInformation("Getting user with id: {id}", id);
            return Ok( new ApiResponse(data: _userService.GetUserById(id).ToString()));
        }

        [HttpGet]
        public IActionResult GetUsersByUsername([FromQuery] string username)
        {
            _logger.LogInformation("Getting users with username: {username}", username);
            return Ok(new ApiResponse(data: _userService.GetUsersByUsername(username)));
        }

        [HttpPut("{id}")] 
        public IActionResult UpdateUser([FromBody] dynamic body)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]);
            
            _logger.LogInformation("Updating user with id: {id}", userId);
            return Ok(new ApiResponse(data: _userService.UpdateUser(userId, body.username, body.name, body.description).ToString()));
        }

        [HttpDelete("{id}")] 
        public IActionResult DeleteUser(long id)
        {
            _logger.LogInformation("Deleting user with id: {id}", id);
            return Ok(new ApiResponse(message: _userService.DeleteUser(id).ToString()));
        }
    }
}