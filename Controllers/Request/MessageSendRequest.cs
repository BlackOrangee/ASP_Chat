using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request
{
    public class MessageSendRequest
    {
        //[Required(ErrorMessage = "ChatId is required")]
        public long ChatId { get; set; }

        public long? ReplyMessageId { get; set; }

        //[RequiredIfMissing(nameof(File))]
        public string? Text { get; set; }

        //[RequiredIfMissing(nameof(Text))]
        public IFormFile? File { get; set; }
    }
}
