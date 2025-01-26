namespace ASP_Chat.Controllers.Request
{
    public class MessageAttachMediaRequest
    {
        public long MessageId { get; set; }
        public IFormFile File { get; set; }
    }
}
