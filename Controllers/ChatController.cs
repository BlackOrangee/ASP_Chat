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

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult CreateChat([FromBody] ChatRequest request, 
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            _logger.LogInformation("Creating new chat");
            request.UserId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.CreateChat(request)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetChatById(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            _logger.LogInformation("Getting chat with id: {Id}", id);
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok( new ApiResponse(data: _chatService.GetChatById(userId, id).ToString()));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetChats([FromQuery] string? name, [FromQuery] string? tag,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            _logger.LogInformation("Getting all chats");
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok( new ApiResponse(data: _chatService.GetChats(userId, name, tag).ToString()));
        }

        [HttpPost("{id}/add/moderators")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult AddModerator(long id, [FromBody] ChatRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            request.ChatId = id;
            request.UserId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.AddModeratorToChat(request).ToString()));
        }

        [HttpPost("{id}/add/users")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult AddUsers(long id, [FromBody] ChatRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            request.ChatId = id;
            request.UserId= _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.AddUsersToChat(request).ToString()));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult UpdateChat(long id, [FromBody] ChatRequest request,
                                            [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            request.ChatId = id;
            request.UserId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.UpdateChatInfo(request).ToString()));
        }

        [HttpGet("{id}/join")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult JoinChat(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.JoinChat(userId, id).ToString()));
        }

        [HttpGet("{id}/leave")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult LeaveChat(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            return Ok(new ApiResponse(data: _chatService.LeaveChat(userId, id).ToString()));
        }
    }
}
