using ASP_Chat.Entity;
using ASP_Chat.Exceptions;

namespace ASP_Chat.Controllers.Request
{
    public class MessageRequest
    {
        public long MessageId { get; set; }
        public long UserId { get; set; }
        public long? ChatId { get; set; }
        public long? ReplyMessageId { get; set; }
        public string? Text { get; set; }
        public ICollection<byte[]>? File { get; set; }

        public void SendMessageValidation()
        {
            if (ChatId == null || ChatId <= 0)
            {
                throw ServerExceptionFactory.FeldAreRequired("chatId");
            }

            if (string.IsNullOrWhiteSpace(Text) && File == null)
            {
                throw ServerExceptionFactory.MessageIsEmpty();
            }
        }
    }
}
