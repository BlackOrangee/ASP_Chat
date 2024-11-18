using ASP_Chat.Entity;
using ASP_Chat.Exception;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service.Impl
{
    public class ChatService : IChatService
    {
        private ApplicationDBContext _context;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ApplicationDBContext context, ILogger<ChatService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void AddModeratorToChat(long adminId, Collection<long> userIds, long chatId)
        {
            throw new NotImplementedException();
        }

        public void AddUsersToChat(long adminId, Collection<long> userIds, long chatId)
        {
            throw new NotImplementedException();
        }

        public Chat CreateChat(long adminId, Collection<long>? users, int chatType, string? tag, string? name, string? description, string? image)
        {
            User? admin = _context.Users.FirstOrDefault(u => u.Id == adminId);
            if (admin == null)
            {
                throw new CustomException("User not found", 
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            ChatType? chatTypeObj = _context.ChatTypes.FirstOrDefault(ct => ct.Id == chatType);
            if (chatTypeObj == null)
            {
                throw new CustomException("Chat type not found", 
                CustomException.ExceptionCodes.ChatTypeNotFound, 
                CustomException.StatusCodes.NotFound);
            }

            HashSet<User> usersSet = _context.Users.Where(u => users.Contains(u.Id)).ToHashSet();

            switch (chatTypeObj.Id)
            {
                case 1:
                    return CreateP2PChat(admin, usersSet.First(), chatTypeObj);
                case 2:
                    return CreateGroupChat(admin, usersSet, chatTypeObj);
                case 3:
                    return CreateChannel(admin, usersSet, chatTypeObj);
            }

            throw new NotImplementedException();
        }

        private Chat CreateP2PChat(User admin, User user, ChatType chatType)
        {
            Chat chat = new Chat()
            {
                Type = chatType,
                Admin = admin,
                Tag = user.Username,
                Name = user.Name,
                Description = user.Description,
                Image = user.Image,
            };

            _context.Chats.Add(chat);
            _context.SaveChanges();

            _context.UserChats.Add(new UserChat { Chat = chat, User = user });
            _context.UserChats.Add(new UserChat { Chat = chat, User = admin });
            _context.SaveChanges();
            return chat;
        }

        public Chat GetChatById(User user, long id)
        {
            throw new NotImplementedException();
        }

        public HashSet<Chat> GetChatsByName(long userId, string name)
        {
            throw new NotImplementedException();
        }

        public HashSet<Chat> GetChatsByTag(long userId, string tag)
        {
            throw new NotImplementedException();
        }

        public HashSet<Chat> GetChatsByUser(long userId)
        {
            throw new NotImplementedException();
        }

        public Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name, string? description, string? image)
        {
            throw new NotImplementedException();
        }
    }
}
