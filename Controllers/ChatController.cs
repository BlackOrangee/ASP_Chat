﻿using ASP_Chat.Controllers.Response;
using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP_Chat.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatService _chatService;
        private readonly IJwtService _jwtService;

        public ChatController(ILogger<ChatController> logger, IChatService chatService, IJwtService jwtService)
        {
            _logger = logger;
            _chatService = chatService;
            _jwtService = jwtService;
        }

        [HttpPost("new")]
        public IActionResult CreateChat([FromBody] dynamic body)
        {
            _logger.LogInformation("Creating new chat");
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);

            return Ok(new ApiResponse(data: _chatService.CreateChat(userId, body.users, body.chatType, 
                                                        body.tag, body.name, body.description, body.image)));
        }

        [HttpGet("{id}")] 
        public IActionResult GetChatById(long id)
        {
            _logger.LogInformation("Getting chat with id: {id}", id);
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok( new ApiResponse(data: _chatService.GetChatById(userId, id).ToString()));
        }

        [HttpGet]
        public IActionResult GetChats([FromQuery] string? name, [FromQuery] string? tag)
        {
            _logger.LogInformation("Getting all chats");
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok( new ApiResponse(data: _chatService.GetChats(userId, name, tag).ToString()));
        }

        [HttpPost("{id}/add/moderators")]
        public IActionResult AddModerator(long id, [FromBody] dynamic body)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _chatService.AddModeratorToChat(userId, body.userIds, id).ToString()));
        }

        [HttpPost("{id}/add/users")]
        public IActionResult AddUsers(long id, [FromBody] dynamic body)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _chatService.AddUsersToChat(userId, body.userIds, id).ToString()));
        }

        [HttpPut("{id}")] 
        public IActionResult UpdateChat(long id, [FromBody] dynamic body)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _chatService.UpdateChatInfo(userId, id, body.tag, body.name, 
                                                                           body.description, body.image).ToString()));
        }

        [HttpGet("{id}/join")] 
        public IActionResult JoinChat(long id, [FromBody] dynamic body)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _chatService.JoinChat(userId, id).ToString()));
        }

        [HttpGet("{id}/leave")] 
        public IActionResult LeaveChat(long id)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _chatService.LeaveChat(userId, id).ToString()));
        }
    }
}
