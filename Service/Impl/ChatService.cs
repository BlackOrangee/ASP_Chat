using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Enums;

namespace ASP_Chat.Service.Impl
{
    public class ChatService : IChatService
    {
        private ApplicationDBContext _context;
        private readonly ILogger<ChatService> _logger;
        private readonly IUserService _userService;

        public ChatService(ApplicationDBContext context, ILogger<ChatService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        private Chat GetChat(long id)
        {
            Chat? chat = _context.Chats.FirstOrDefault(c => c.Id == id);
            if (chat == null)
            {
                throw ServerExceptionFactory.ChatNotFound();
            }

            return chat;
        }

        public string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId)
        {
            _logger.LogDebug("Adding moderators to chat: {chatId}", chatId);

            Chat? chat = GetChat(chatId);

            if (chat.Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotHaveModerators();
            }

            User admin = _userService.GetUserById(adminId);

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw ServerExceptionFactory.UsersNotFound();
            }

            if (chat.Admin != admin)
            {
                throw ServerExceptionFactory.UserNotAdmin();
            }

            foreach (User user in usersSet)
            {
                if (!chat.Users.Contains(user))
                {
                    throw ServerExceptionFactory.UserNotInChat();
                }
            }

            if (chat.Moderators == null)
            {
                chat.Moderators = new HashSet<User>();
            }
       
            foreach (User user in usersSet)
            {
                if (chat.Moderators.Contains(user))
                {
                    throw ServerExceptionFactory.UserAlreadyModerator();
                }
            }

            foreach (User user in usersSet)
            {
                chat.Moderators.Add(user);
                user.ModeratedChats.Add(chat);
                _context.Users.Update(user);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Moderators added successfully";
        }

        public string AddUsersToChat(long chatUserId, ICollection<long> userIds, long chatId)
        {
            _logger.LogDebug("Adding users to chat: {chatId}", chatId);

            Chat? chat = GetChat(chatId);

            if (chat.Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotHaveUsers();
            }

            User chatUser = _userService.GetUserById(chatUserId);

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw ServerExceptionFactory.UsersNotFound();
            }

            if (!chat.Users.Contains(chatUser))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            foreach (User user in usersSet)
            {
                if (chat.Users.Contains(user))
                {
                    throw ServerExceptionFactory.UserNotInChat();
                }
            }

            foreach (User user in usersSet)
            {
                chat.Users.Add(user);
                user.Chats.Add(chat);
                _context.Users.Update(user);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Users added successfully";
        }

        public Chat CreateChat(long adminId, ICollection<long>? users, int chatType, 
            string? tag, string? name, string? description, string? image)
        {
            _logger.LogDebug("Creating chat with admin id: {adminId}", adminId);
            User admin = _userService.GetUserById(adminId);

            ChatType? chatTypeObj = _context.ChatTypes.FirstOrDefault(ct => ct.Id == chatType);
            if (chatTypeObj == null)
            {
                throw ServerExceptionFactory.ChatTypeNotFound();
            }

            HashSet<User> usersSet = _context.Users.Where(u => users.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw ServerExceptionFactory.UsersNotFound();
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
            string? tag, string name, string? description, Media? image)
        {
            _logger.LogDebug("Creating channel with admin id: {adminId}", admin.Id);
            if (name == null || string.IsNullOrEmpty(name))
            {
                throw ServerExceptionFactory.ChannelNameIsEmpty();
            }

            if (string.IsNullOrEmpty(description))
            {
                description = "Channel description";
            }

            if (string.IsNullOrEmpty(tag))
            {
                throw ServerExceptionFactory.ChannelTagIsEmpty();
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


            chat.Users.Add(admin);

            foreach (User user in users)
            {
                chat.Users.Add(user);
            }

            _context.Chats.Add(chat);
            _context.SaveChanges();
            return chat;
        }

        private Chat CreateGroupChat(User admin, HashSet<User> users, ChatType chatType,
            string name, string? description, Media? image)
        {
            _logger.LogDebug("Creating group with admin id: {adminId}", admin.Id);
            if (name == null || string.IsNullOrEmpty(name))
            {
                throw ServerExceptionFactory.GroupNameIsEmpty();
            }

            if (string.IsNullOrEmpty(description))
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


            chat.Users.Add(admin);

            foreach (User user in users)
            {
                chat.Users.Add(user);
            }

            _context.Chats.Add(chat);
            _context.SaveChanges();
            return chat;
        }

        private Chat CreateP2PChat(User admin, User user, ChatType chatType)
        {
            _logger.LogDebug("Creating p2p chat with admin id: {adminId}", admin.Id);
            Chat chat = new Chat()
            {
                Type = chatType,
                Admin = admin,
            };

            chat.Users.Add(admin);
            chat.Users.Add(user);

            _context.Chats.Add(chat);
            _context.SaveChanges();
            return chat;
        }

        public Chat GetChatById(long userId, long chatId)
        {
            _logger.LogDebug("Getting chat with id: {chatId}", chatId);
            User user = _userService.GetUserById(userId);

            Chat? chat = GetChat(chatId);

            if (!chat.Users.Contains(user))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            return chat;
        }

        public ICollection<Chat> GetChatsByName(long userId, string name)
        {
            _logger.LogDebug("Getting chats with name: {name}", name);
            User user = _userService.GetUserById(userId);

            HashSet<Chat> userGroupAndChanels = _context.Chats.Where(
                    c => c.Type.Id != (long)ChatTypes.P2P
                    && c.Users.Contains(user)
                    && c.Name.Contains(name)
                ).ToHashSet();
            
            HashSet<Chat> userPersonalChats = _context.Chats.Where(
                    c => c.Type.Id == (long)ChatTypes.P2P
                    && c.Users.Contains(user)
                    && c.Users.FirstOrDefault(u => u.Id != userId).Name.Contains(name)
                ).ToHashSet();

            HashSet<Chat> ChanelsToJoin = _context.Chats.Where(
                    c => c.Type.Id == (long)ChatTypes.Channel
                    && c.Name.Contains(name)
                ).ToHashSet();

            return new HashSet<Chat>(userPersonalChats.Concat(
                userGroupAndChanels.Concat(ChanelsToJoin).ToHashSet()).ToHashSet());
        }

        public ICollection<Chat> GetChatsByTag(long userId, string tag)
        {
            _logger.LogDebug("Getting chats with tag: {tag}", tag);
            User user = _userService.GetUserById(userId);

            HashSet<Chat> userPersonalChats = _context.Chats.Where(
                    c => c.Type.Id == (long)ChatTypes.P2P
                    && c.Users.Contains(user)
                    && c.Users.FirstOrDefault(u => u.Id != userId).Username.Contains(tag)
                ).ToHashSet();

            HashSet<Chat> Chanels = _context.Chats.Where(
                    c => c.Type.Id == (long)ChatTypes.Channel
                    && c.Tag.Contains(tag)
                ).ToHashSet();

            return new HashSet<Chat>(userPersonalChats.Concat(Chanels).ToHashSet());
        }

        public ICollection<Chat> GetChatsByUser(long userId)
        {
            _logger.LogDebug("Getting chats with user id: {userId}", userId);
            User user = _userService.GetUserById(userId);

            return user.Chats;
        }

        public Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name,
            string? description, Media? image)
        {
            _logger.LogDebug("Updating chat with id: {chatId}", chatId);
            User user = _userService.GetUserById(adminId);

            Chat? chat = GetChat(chatId);

            if (chat.Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotBeUpdated();
            }

            if (chat.Admin.Id != adminId)
            {
                throw ServerExceptionFactory.UserNotAdmin();
            }

            if (!string.IsNullOrEmpty(tag) && chat.Type.Id == (long)ChatTypes.Channel)
            {
                chat.Tag = tag;
            }

            if (!string.IsNullOrEmpty(name))
            {
                chat.Name = name;
            }

            if (!string.IsNullOrEmpty(description))
            {
                chat.Description = description;
            }

            if (image != null)
            {
                chat.Image = image;
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return chat;
        }

        public ICollection<Chat> GetChats(long userId, string? name, string? tag)
        {
            _logger.LogDebug("Getting chats with user id: {userId}", userId);

            if (name != null)
            {
                return GetChatsByName(userId, name);
            }
            else if (tag != null)
            {
                return GetChatsByTag(userId, tag);
            }
                
            return GetChatsByUser(userId);
        }

        public string JoinChat(long userId, long chatId)
        {
            _logger.LogDebug("Joining chat with id: {chatId}", chatId);
            User user = _userService.GetUserById(userId);

            Chat chat = GetChat(chatId);

            if (chat.Users.Contains(user))
            {
                throw ServerExceptionFactory.UserAlreadyInChat();
            }

            if (chat.Type.Id == (long)ChatTypes.Channel)
            {
                chat.Users.Add(user);
                _context.Chats.Update(chat);
                _context.SaveChanges();

                return "Joined successfully";
            }
            
            throw ServerExceptionFactory.ChatNotPublic();
        }

        public string LeaveChat(long userId, long chatId)
        {
            _logger.LogDebug("Leaving chat with id: {chatId}", chatId);
            User user = _userService.GetUserById(userId);
            Chat chat = GetChat(chatId);

            if (!chat.Users.Contains(user))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            if (chat.Type.Id == (long)ChatTypes.P2P)
            {
                chat.Users.Remove(user);
                if (chat.Users.Count == 1)
                {
                    _context.Chats.Remove(chat);
                }
                else
                {
                    chat.Admin = chat.Users.First(u => u.Id != userId);
                }
            }
            else
            {
                if (chat.Users.Count == 1)
                {
                    chat.Users.Remove(user);
                    _context.Chats.Remove(chat);
                }
                else if (chat.Admin.Id == user.Id)
                {
                    throw ServerExceptionFactory.UserCanNotLeaveChatAsAdmin();
                }
                chat.Users.Remove(user);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Left successfully";
        }
    }
}
