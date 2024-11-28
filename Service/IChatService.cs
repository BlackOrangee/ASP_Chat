using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IChatService
    {
        Chat CreateChat(ChatRequest request);
        Chat UpdateChatInfo(ChatRequest request);
        string AddModeratorToChat(ChatRequest request);
        string AddUsersToChat(ChatRequest request);
        Chat GetChatById(long userId, long chatId);
        ICollection<Chat> GetChats(long userId, string? name, string? tag);
        string JoinChat(long userId, long chatId);
        string LeaveChat(long userId, long chatId);
    }
}
