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
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IMessageService _messageService;

        public MessageController(ILogger<MessageController> logger, IMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] dynamic body)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.SendMessage(userId, body.chatId, 
                                                        body.replyMessageId, body.text, body.file).ToString()));
        }

        [HttpPatch("{id}")] 
        public IActionResult EditMessage(long id, [FromBody] dynamic body)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.EditMessage(userId, id, body.text).ToString()));
        }

        [HttpDelete("{id}")] 
        public IActionResult DeleteMessage(long id)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.DeleteMessage(userId, id).ToString()));
        }

        [HttpGet("{id}")] 
        public IActionResult GetMessageById(long id)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.GetMessage(userId, id).ToString()));
        }

        [HttpGet("chat/{id}")] 
        public IActionResult GetMessagesByChatId(long id, [FromQuery] long? lastMessageId)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]); 
            return Ok(new ApiResponse(data: _messageService.GetMessages(userId, id, lastMessageId).ToString()));
        }

        [HttpPut("{id}")] 
        public IActionResult LikeMessage(long id)
        {
            long userId = AuthService.GetUserIdFromToken(Request.Headers["Authorization"]);
            _messageService.SetReadedMessageStatus(userId, id);
            return Ok(new ApiResponse(success: true));
        }
    }
}