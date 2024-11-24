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
                throw new ServerException("Chat not found",
                ServerException.ExceptionCodes.ChatNotFound,
                ServerException.StatusCodes.NotFound);
            }

            return chat;
        }

        public string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId)
        {
            _logger.LogDebug("Adding moderators to chat: {chatId}", chatId);

            Chat? chat = GetChat(chatId);

            if (chat.Type.Id == (long)EChatType.P2P)
            {
                throw new ServerException("P2P chat can't have moderators",
                ServerException.ExceptionCodes.ChatCanNotHaveModerators,
                ServerException.StatusCodes.BadRequest);
            }

            User admin = _userService.GetUserById(adminId);

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new ServerException("No found users to make moderators",
                ServerException.ExceptionCodes.UsersNotFound,
                ServerException.StatusCodes.NotFound);
            }

            if (chat.Admin != admin)
            {
                throw new ServerException("User is not admin of this chat",
                ServerException.ExceptionCodes.UserNotAdmin,
                ServerException.StatusCodes.BadRequest);
            }

            foreach (User user in usersSet)
            {
                if (!chat.Users.Contains(user))
                {
                    throw new ServerException("User " + user.Name + " not in this chat",
                    ServerException.ExceptionCodes.UserNotInChat,
                    ServerException.StatusCodes.BadRequest);
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
                    throw new ServerException("User " + user.Name + " is already moderator of this chat",
                    ServerException.ExceptionCodes.UserAlreadyModerator,
                    ServerException.StatusCodes.BadRequest);
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

            if (chat.Type.Id == (long)EChatType.P2P)
            {
                throw new ServerException("P2P chat can't have morer users",
                ServerException.ExceptionCodes.ChatCanNotHaveUsers,
                ServerException.StatusCodes.BadRequest);
            }

            User chatUser = _userService.GetUserById(chatUserId);

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new ServerException("No found users to add in chat",
                ServerException.ExceptionCodes.UsersNotFound,
                ServerException.StatusCodes.NotFound);
            }

            if (!chat.Users.Contains(chatUser))
            {
                throw new ServerException("User is not in this chat",
                ServerException.ExceptionCodes.UserNotInChat,
                ServerException.StatusCodes.BadRequest);
            }

            foreach (User user in usersSet)
            {
                if (chat.Users.Contains(user))
                {
                    throw new ServerException("User " + user.Name + " already in this chat",
                    ServerException.ExceptionCodes.UserNotInChat,
                    ServerException.StatusCodes.BadRequest);
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
                throw new ServerException("Chat type not found", 
                ServerException.ExceptionCodes.ChatTypeNotFound, 
                ServerException.StatusCodes.NotFound);
            }

            HashSet<User> usersSet = _context.Users.Where(u => users.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new ServerException("No found users to create chat",
                ServerException.ExceptionCodes.UsersNotFound, 
                ServerException.StatusCodes.NotFound);
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
                throw new ServerException("Channel name is empty",
                    ServerException.ExceptionCodes.ChannelNameIsEmpty,
                    ServerException.StatusCodes.BadRequest);
            }

            if (string.IsNullOrEmpty(description))
            {
                description = "Channel description";
            }

            if (string.IsNullOrEmpty(tag))
            {
                throw new ServerException("Channel tag is empty",
                    ServerException.ExceptionCodes.ChannelTagIsEmpty,
                    ServerException.StatusCodes.BadRequest);
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
                throw new ServerException("Group name is empty",
                    ServerException.ExceptionCodes.GroupNameIsEmpty,
                    ServerException.StatusCodes.BadRequest);
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
                throw new ServerException("User is not in this chat",
                ServerException.ExceptionCodes.UserNotInChat,
                ServerException.StatusCodes.BadRequest);
            }

            return chat;
        }

        public ICollection<Chat> GetChatsByName(long userId, string name)
        {
            _logger.LogDebug("Getting chats with name: {name}", name);
            User user = _userService.GetUserById(userId);

            HashSet<Chat> userGroupAndChanels = _context.Chats.Where(
                    c => c.Type.Id != (long)EChatType.P2P
                    && c.Users.Contains(user)
                    && c.Name.Contains(name)
                ).ToHashSet();
            
            HashSet<Chat> userPersonalChats = _context.Chats.Where(
                    c => c.Type.Id == (long)EChatType.P2P
                    && c.Users.Contains(user)
                    && c.Users.FirstOrDefault(u => u.Id != userId).Name.Contains(name)
                ).ToHashSet();

            HashSet<Chat> ChanelsToJoin = _context.Chats.Where(
                    c => c.Type.Id == (long)EChatType.Channel
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
                    c => c.Type.Id == (long)EChatType.P2P
                    && c.Users.Contains(user)
                    && c.Users.FirstOrDefault(u => u.Id != userId).Username.Contains(tag)
                ).ToHashSet();

            HashSet<Chat> Chanels = _context.Chats.Where(
                    c => c.Type.Id == (long)EChatType.Channel
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

            if (chat.Type.Id == (long)EChatType.P2P)
            {
                throw new ServerException("P2P chat can't be updated",
                ServerException.ExceptionCodes.ChatCanNotBeUpdated,
                ServerException.StatusCodes.BadRequest);
            }

            if (chat.Admin.Id != adminId)
            {
                throw new ServerException("User is not admin of this chat",
                ServerException.ExceptionCodes.UserNotAdmin,
                ServerException.StatusCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(tag) && chat.Type.Id == (long)EChatType.Channel)
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
    }
}
