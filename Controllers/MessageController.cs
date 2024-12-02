using ASP_Chat.Controllers.Request;
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
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult SendMessage([FromBody] MessageSendRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _messageService.SendMessage(userId, request)));
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult EditMessage(long id, [FromBody] MessageEditRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long messageId = id;
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _messageService.EditMessage(userId, messageId, request)));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult DeleteMessage(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader); 
            return Ok(new ApiResponse(data: _messageService.DeleteMessage(userId, id)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetMessageById(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader); 
            return Ok(new ApiResponse(data: _messageService.GetMessage(userId, id)));
        }

        [HttpGet("chat/{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetMessagesByChatId(long id, [FromQuery] long? lastMessageId,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader); 
            return Ok(new ApiResponse(data: _messageService.GetMessages(userId, id, lastMessageId)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult LikeMessage(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            _messageService.SetReadedMessageStatus(userId, id);
            return Ok(new ApiResponse(success: true));
        }
    }
}