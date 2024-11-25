using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(long adminId, ICollection<long>? users, int chatType, string? tag, string? name, string? description, string? image);
        Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name, string? description, Media? image);
        string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId);
        string AddUsersToChat(long adminId, ICollection<long> userIds, long chatId);
        Chat GetChatById(long userId, long chatId);
        ICollection<Chat> GetChats(long userId, string? name, string? tag);
        string JoinChat(long userId, long chatId);
        string LeaveChat(long userId, long chatId);
    }
}
