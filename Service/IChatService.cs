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
        HashSet<Chat> GetChatsByName(long userId, string name);
        HashSet<Chat> GetChatsByTag(long userId, string tag);
        Chat GetChatById(User user, long id);
        HashSet<Chat> GetChatsByUser(long userId);
        
    }
}
