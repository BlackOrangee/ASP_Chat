using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(long adminId, Collection<long>? users, int chatType, string? tag, string? name, string? description, string? image);
        Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name, string? description, string? image);
        void AddModeratorToChat(long adminId, Collection<long> userIds, long chatId);
        void AddUsersToChat(long adminId, Collection<long> userIds, long chatId);
        Collection<Chat> GetChatsByName(long userId, string name);
        Collection<Chat> GetChatByTag(long userId, string tag);
        Chat GetChatById(User user, long id);
        Collection<Chat> GetChatsByUser(long userId);
        
    }
}
