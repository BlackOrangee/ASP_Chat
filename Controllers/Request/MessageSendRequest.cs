using ASP_Chat.Controllers.ValidationAttributes.RequiredAtributes;
using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request
{
    public class MessageSendRequest
    {
        [Required(ErrorMessage = "ChatId is required")]
        public long ChatId { get; set; }

        public long? ReplyMessageId { get; set; }

        [RequiredIfMissing(nameof(File))]
        public string? Text { get; set; }

        [RequiredIfMissing(nameof(Text))]
        public ICollection<byte[]>? File { get; set; }
    }
}
