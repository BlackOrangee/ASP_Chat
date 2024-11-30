using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(long userId, ChatCreateRequest request);
        Chat UpdateChatInfo(long userId, long chatId, ChatUpdateRequest request);
        string AddModeratorToChat(long userId, long chatId, ChatAddUsersRequest request);
        string AddUsersToChat(long userId, long chatId, ChatAddUsersRequest request);
        Chat GetChatById(long userId, long chatId);
        ICollection<Chat> GetChats(long userId, string? name, string? tag);
        string JoinChat(long userId, long chatId);
        string LeaveChat(long userId, long chatId);
    }
}
