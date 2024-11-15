using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IMessageService
    {
        Message SendMessage(long userId, long chatId, long? replyMessageId, string? text, Collection<byte[]>? file);
        Message EditMessage(long userId, long messageId, string text);
        bool DeleteMessage(long userId, long messageId);
        Message GetMessage(long messageId);
        Collection<Message> GetMessages(long chatId);
    }
}
