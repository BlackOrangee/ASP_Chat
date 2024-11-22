using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(long adminId, ICollection<long>? users, int chatType, string? tag, string? name, string? description, string? image);
        Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name, string? description, string? image);
        string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId);
        string AddUsersToChat(long adminId, ICollection<long> userIds, long chatId);
        HashSet<Chat> GetChatsByName(long userId, string name);
        HashSet<Chat> GetChatsByTag(long userId, string tag);
        Chat GetChatById(User user, long id);
        HashSet<Chat> GetChatsByUser(long userId);
        
    }
}
