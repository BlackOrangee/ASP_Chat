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

        public string AddModeratorToChat(long adminId, ICollection<long> userIds, long chatId)
        {
            _logger.LogDebug("Adding moderators to chat: {chatId}", chatId);

            Chat? chat = _context.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
            {
                throw new CustomException("Chat not found",
                CustomException.ExceptionCodes.ChatNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (chat.Type.Id == 1)
            {
                throw new CustomException("P2P chat can't have moderators",
                CustomException.ExceptionCodes.ChatCanNotHaveModerators,
                CustomException.StatusCodes.BadRequest);
            }

            User? admin = _context.Users.FirstOrDefault(u => u.Id == adminId);
            if (admin == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new CustomException("No found users to make moderators",
                CustomException.ExceptionCodes.UsersNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (chat.Admin != admin)
            {
                throw new CustomException("User is not admin of this chat",
                CustomException.ExceptionCodes.UserNotAdmin,
                CustomException.StatusCodes.BadRequest);
            }

            foreach (User user in usersSet)
            {
                if (!chat.Users.Contains(user))
                {
                    throw new CustomException("User " + user.Name + " not in this chat",
                    CustomException.ExceptionCodes.UserNotInChat,
                    CustomException.StatusCodes.BadRequest);
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
                    throw new CustomException("User " + user.Name + " is already moderator of this chat",
                    CustomException.ExceptionCodes.UserAlreadyModerator,
                    CustomException.StatusCodes.BadRequest);
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

            Chat? chat = _context.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
            {
                throw new CustomException("Chat not found",
                CustomException.ExceptionCodes.ChatNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (chat.Type.Id == 1)
            {
                throw new CustomException("P2P chat can't have morer users",
                CustomException.ExceptionCodes.ChatCanNotHaveUsers,
                CustomException.StatusCodes.BadRequest);
            }

            User? chatUser = _context.Users.FirstOrDefault(u => u.Id == chatUserId);
            if (chatUser == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            HashSet<User> usersSet = _context.Users.Where(u => userIds.Contains(u.Id)).ToHashSet();

            if (usersSet == null || usersSet.Count == 0)
            {
                throw new CustomException("No found users to add in chat",
                CustomException.ExceptionCodes.UsersNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (!chat.Users.Contains(chatUser))
            {
                throw new CustomException("User is not in this chat",
                CustomException.ExceptionCodes.UserNotInChat,
                CustomException.StatusCodes.BadRequest);
            }

            foreach (User user in usersSet)
            {
                if (chat.Users.Contains(user))
                {
                    throw new CustomException("User " + user.Name + " already in this chat",
                    CustomException.ExceptionCodes.UserNotInChat,
                    CustomException.StatusCodes.BadRequest);
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

        public Chat CreateChat(long adminId, ICollection<long>? users, int chatType, string? tag, string? name, string? description, string? image)
        {
            _logger.LogDebug("Creating chat with admin id: {adminId}", adminId);
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
            _logger.LogDebug("Creating channel with admin id: {adminId}", admin.Id);
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
            string? name, string? description, Media? image)
        {
            _logger.LogDebug("Creating group with admin id: {adminId}", admin.Id);
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
