using ASP_Chat.Entity;
using ASP_Chat.Exception;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

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

        public Chat CreateChat(long adminId, ICollection<long>? users, int chatType, 
            string? tag, string? name, string? description, string? image)
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
            string? tag, string name, string? description, Media? image)
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
            string name, string? description, Media? image)
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

        public Chat GetChatById(long userId, long chatId)
        {
            _logger.LogDebug("Getting chat with id: {chatId}", chatId);
            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            Chat? chat = _context.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
            {
                throw new CustomException("Chat not found",
                CustomException.ExceptionCodes.ChatNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (!chat.Users.Contains(user))
            {
                throw new CustomException("User is not in this chat",
                CustomException.ExceptionCodes.UserNotInChat,
                CustomException.StatusCodes.BadRequest);
            }

            return chat;
        }

        public ICollection<Chat> GetChatsByName(long userId, string name)
        {
            _logger.LogDebug("Getting chats with name: {name}", name);
            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            HashSet<Chat> userGroupAndChanels = _context.Chats.Where(c => 
            c.Type.Id != 1 
            && c.Users.Contains(user) 
            && c.Name.Contains(name))
                .ToHashSet();
            
            HashSet<Chat> userPersonalChats = _context.Chats.Where(c => 
            c.Type.Id == 1 
            && c.Users.Contains(user) 
            && c.Users.FirstOrDefault(u => u.Id != userId).Name.Contains(name))
                .ToHashSet();

            HashSet<Chat> ChanelsToJoin = _context.Chats.Where(c => 
            c.Type.Id == 3
            && c.Name.Contains(name))
                .ToHashSet();

            return new HashSet<Chat>(userPersonalChats.Concat(
                userGroupAndChanels.Concat(ChanelsToJoin).ToHashSet()).ToHashSet());
        }

        public ICollection<Chat> GetChatsByTag(long userId, string tag)
        {
            _logger.LogDebug("Getting chats with tag: {tag}", tag);
            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            HashSet<Chat> userPersonalChats = _context.Chats.Where(c =>
            c.Type.Id == 1
            && c.Users.Contains(user)
            && c.Users.FirstOrDefault(u => u.Id != userId).Username.Contains(tag))
                .ToHashSet();

            HashSet<Chat> Chanels = _context.Chats.Where(c =>
            c.Type.Id == 3
            && c.Tag.Contains(tag))
                .ToHashSet();

            return new HashSet<Chat>(userPersonalChats.Concat(Chanels).ToHashSet());
        }

        public ICollection<Chat> GetChatsByUser(long userId)
        {
            _logger.LogDebug("Getting chats with user id: {userId}", userId);
            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            return user.Chats;
        }

        public Chat UpdateChatInfo(long adminId, long chatId, string? tag, string? name,
            string? description, Media? image)
        {
            _logger.LogDebug("Updating chat with id: {chatId}", chatId);
            User? user = _context.Users.FirstOrDefault(u => u.Id == adminId);
            if (user == null)
            {
                throw new CustomException("User not found",
                CustomException.ExceptionCodes.UserNotFound,
                CustomException.StatusCodes.NotFound);
            }

            Chat? chat = _context.Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
            {
                throw new CustomException("Chat not found",
                CustomException.ExceptionCodes.ChatNotFound,
                CustomException.StatusCodes.NotFound);
            }

            if (chat.Type.Id == 1)
            {
                throw new CustomException("P2P chat can't be updated",
                CustomException.ExceptionCodes.ChatCanNotBeUpdated,
                CustomException.StatusCodes.BadRequest);
            }

            if (chat.Admin.Id != adminId)
            {
                throw new CustomException("User is not admin of this chat",
                CustomException.ExceptionCodes.UserNotAdmin,
                CustomException.StatusCodes.BadRequest);
            }

            if (!string.IsNullOrEmpty(tag) && chat.Type.Id == 3)
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
