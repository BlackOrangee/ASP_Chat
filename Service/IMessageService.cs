using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IMessageService
    {
        Message SendMessage(long userId, MessageSendRequest request);
        Message EditMessage(long userId, long messageId, MessageEditRequest request);
        string DeleteMessage(long userId, long messageId);
        Message GetMessage(long userId, long messageId);
        ICollection<Message> GetMessages(long userId, long chatId, long? lastMessageId);
        Message SetReadedMessageStatus(long userId, long messageId);
        Task<Message> AttachMediaToMessage(long userId, MessageAttachMediaRequest mediaAttachRequest);
    }
}
