using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IMessageService
    {
        Message SendMessage(long userId, long chatId, long? replyMessageId, string? text, ICollection<byte[]>? file);
        Message EditMessage(long userId, long messageId, string text);
        string DeleteMessage(long userId, long messageId);
        Message GetMessage(long userId, long messageId);
        ICollection<Message> GetMessages(long userId, long chatId, long? lastMessageId);
        void SetReadedMessageStatus(long userId, long messageId);
    }
}
