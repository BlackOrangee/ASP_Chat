using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IMessageService
    {
        Message SendMessage(MessageRequest request);
        Message EditMessage(MessageRequest request);
        string DeleteMessage(long userId, long messageId);
        Message GetMessage(long userId, long messageId);
        ICollection<Message> GetMessages(long userId, long chatId, long? lastMessageId);
        void SetReadedMessageStatus(long userId, long messageId);
    }
}
