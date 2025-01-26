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
    public class MediaController : ControllerBase
    {
        private readonly ILogger<MediaController> _logger;
        private readonly IMediaService _mediaService;
        private readonly IJwtService _jwtService;

        public MediaController(ILogger<MediaController> logger,
                               IMediaService mediaService,
                               IJwtService jwtService)
        {
            _logger = logger;
            _mediaService = mediaService;
            _jwtService = jwtService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult GetMediaLinkById(long id, [FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            _logger.LogInformation("Getting media with id: {Id}", id);
            long userId = _jwtService.GetUserIdFromToken(authorizationHeader);
            var mediaLink = _mediaService.GetFileLink(id, userId, 604800);
            return Ok(new ApiResponse(data: mediaLink));
        }
    }
}
