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

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new CustomException("No found users to create chat",
                CustomException.ExceptionCodes.UsersNotFound, 
                CustomException.StatusCodes.NotFound);
            }

            // TODO: add media upload
             Media? imageMedia = new Media();

            switch (chatTypeObj.Id)
            {
                case 1:
                    return CreateP2PChat(admin, usersSet.First(), chatTypeObj);
                case 2:
                    return CreateGroupChat(admin, usersSet, chatTypeObj, name, description, imageMedia);
                case 3:
                    return CreateChannel(admin, usersSet, chatTypeObj, tag, name, description, imageMedia);
            }

            throw new NotImplementedException();
        }

        private Chat CreateChannel(User admin, HashSet<User> users, ChatType chatType,
            string? tag, string? name, string? description, Media? image)
        {
            if (name == null || string.IsNullOrEmpty(name))
            {
                throw new CustomException("Channel name is empty",
                    CustomException.ExceptionCodes.ChannelNameIsEmpty,
                    CustomException.StatusCodes.BadRequest);
            }

            if (description == null || string.IsNullOrEmpty(description))
            {
                description = "Channel description";
            }

            if (tag == null || string.IsNullOrEmpty(tag))
            {
                throw new CustomException("Channel tag is empty",
                    CustomException.ExceptionCodes.ChannelTagIsEmpty,
                    CustomException.StatusCodes.BadRequest);
            }

            Chat chat = new Chat()
            {
                Type = chatType,
                Admin = admin,
                Tag = tag,
                Name = name,
                Description = description,
                Image = image
            };

            _context.Chats.Add(chat);
            _context.SaveChanges();

            _context.UserChats.Add(new UserChat { Chat = chat, User = admin });

            foreach (User user in users)
            {
                _context.UserChats.Add(new UserChat { Chat = chat, User = user });
            }
            _context.SaveChanges();
            return chat;
        }

        private Chat CreateGroupChat(User admin, HashSet<User> users, ChatType chatType,
            string? name, string? description, Media? image)
        {
            if (name == null || string.IsNullOrEmpty(name))
            {
                throw new CustomException("Group name is empty",
                    CustomException.ExceptionCodes.GroupNameIsEmpty,
                    CustomException.StatusCodes.BadRequest);
            }

            if (description == null || string.IsNullOrEmpty(description))
            {
                description = "Group description";
            }

            Chat chat = new Chat()
            {
                Type = chatType,
                Admin = admin,
                Name = name,
                Description = description,
                Image = image
            };

            _context.Chats.Add(chat);
            _context.SaveChanges();

            _context.UserChats.Add(new UserChat { Chat = chat, User = admin });

            foreach (User user in users)
            {
                _context.UserChats.Add(new UserChat { Chat = chat, User = user });
            }
            _context.SaveChanges();
            return chat;
        }

        private Chat CreateP2PChat(User admin, User user, ChatType chatType)
        {
            Chat chat = new Chat()
            {
                Type = chatType,
                Admin = admin,
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
