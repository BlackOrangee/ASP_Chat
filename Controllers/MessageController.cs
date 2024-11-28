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
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IMessageService _messageService;
        private readonly IJwtService _jwtService;

        public MessageController(ILogger<MessageController> logger, 
            IMessageService messageService, IJwtService jwtService)
        {
            _logger = logger;
            _messageService = messageService;
            _jwtService = jwtService;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] MessageRequest request)
        {
            request.UserId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _messageService.SendMessage(request).ToString()));
        }

        [HttpPatch("{id}")] 
        public IActionResult EditMessage(long id, [FromBody] MessageRequest request)
        {
            request.MessageId = id;
            request.UserId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            return Ok(new ApiResponse(data: _messageService.EditMessage(request).ToString()));
        }

        [HttpDelete("{id}")] 
        public IActionResult DeleteMessage(long id)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.DeleteMessage(userId, id).ToString()));
        }

        [HttpGet("{id}")] 
        public IActionResult GetMessageById(long id)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.GetMessage(userId, id).ToString()));
        }

        [HttpGet("chat/{id}")] 
        public IActionResult GetMessagesByChatId(long id, [FromQuery] long? lastMessageId)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.GetMessages(userId, id, lastMessageId).ToString()));
        }

        [HttpPut("{id}")] 
        public IActionResult LikeMessage(long id)
        {
            long userId = _jwtService.GetUserIdFromToken(Request.Headers["Authorization"]);
            _messageService.SetReadedMessageStatus(userId, id);
            return Ok(new ApiResponse(success: true));
        }
    }
}