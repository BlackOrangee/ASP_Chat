using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(long adminId, ICollection<long>? users, int chatType, string? tag, string? name, string? description, string? image);
        Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name, string? description, Media? image);
        string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId);
        string AddUsersToChat(long adminId, ICollection<long> userIds, long chatId);
        ICollection<Chat> GetChatsByName(long userId, string name);
        ICollection<Chat> GetChatsByTag(long userId, string tag);
        Chat GetChatById(long userId, long chatId);
        ICollection<Chat> GetChatsByUser(long userId);
        
    }
}
