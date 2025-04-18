﻿using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Enums;
using ASP_Chat.Exceptions;
using ASP_Chat.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASP_Chat.Service.Impl
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<ChatService> _logger;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(ApplicationDBContext context,
                           ILogger<ChatService> logger,
                           IUserService userService,
                           IMediaService mediaService,
                           IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
            _mediaService = mediaService;
            _hubContext = hubContext;
        }

        private Chat GetChat(long id)
        {
            Chat? chat = _context.Chats
                                    .AsSplitQuery()
                                    .Include(c => c.Type)
                                    .Include(c => c.Users)
                                    .ThenInclude(u => u.Image)
                                    .Include(c => c.Moderators)
                                    .Include(c => c.Messages)
                                    .ThenInclude(m => m.Media)
                                    .Include(c => c.Image)
                                    .FirstOrDefault(c => c.Id == id);

            if (chat == null)
            {
                throw ServerExceptionFactory.ChatNotFound();
            }

            return chat;
        }

        private static void ThrowExceptionIfUsersNotFound(ICollection<User> users)
        {
            if (users.Count == 0)
            {
                throw ServerExceptionFactory.UsersNotFound();
            }
        }

        private bool IsTagTaken(string tag)
        {
            return _context.Chats.FirstOrDefault(c => c.Tag == tag) != null
                || _context.Users.FirstOrDefault(u => u.Username == tag) != null;
        }

        public string AddModeratorToChat(long userId, long chatId, ChatAddUsersRequest request)
        {
            _logger.LogDebug("Adding moderators to chat: {ChatId}", chatId);

            Chat? chat = GetChat(chatId);

            if (chat.IsChatP2P())
            {
                throw ServerExceptionFactory.ChatCanNotHaveModerators();
            }

            User admin = _userService.GetUserById(userId);

            if (!chat.IsUserAdmin(admin))
            {
                throw ServerExceptionFactory.UserNotAdmin();
            }

            HashSet<User> usersSet = _context.Users.Where(u => request.Users.Contains(u.Id)).ToHashSet();

            ThrowExceptionIfUsersNotFound(usersSet);

            foreach (User user in usersSet)
            {
                if (!chat.IsUserInChat(user))
                {
                    throw ServerExceptionFactory.UserNotInChat();
                }

                if (chat.IsUserModerator(user))
                {
                    throw ServerExceptionFactory.UserAlreadyModerator();
                }
                chat.AddModerator(user);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Moderators added successfully";
        }

        public string AddUsersToChat(long userId, long chatId, ChatAddUsersRequest request)
        {
            _logger.LogDebug("Adding users to chat: {ChatId}", chatId);

            Chat? chat = GetChat(chatId);

            if (chat.IsChatP2P())
            {
                throw ServerExceptionFactory.ChatCanNotHaveUsers();
            }

            User chatUser = _userService.GetUserById(userId);

            if (!chat.IsUserInChat(chatUser))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            HashSet<User> usersSet = _context.Users.Where(u => request.Users.Contains(u.Id)).ToHashSet();

            ThrowExceptionIfUsersNotFound(usersSet);

            foreach (User user in usersSet)
            {
                if (chat.IsUserInChat(user))
                {
                    throw ServerExceptionFactory.UserAlreadyInChat();
                }
                chat.AddUser(user);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Users added successfully";
        }

        public Chat CreateChat(long userId, ChatCreateRequest request)
        {
            _logger.LogDebug("Creating chat with admin id: {AdminId}", userId);
            User admin = _userService.GetUserById(userId);

            ChatType? chatTypeObj = _context.ChatTypes.FirstOrDefault(ct => ct.Id == request.TypeId);
            if (chatTypeObj == null)
            {
                throw ServerExceptionFactory.ChatTypeNotFound();
            }

            HashSet<User> usersSet = _context.Users.Where(u => request.Users.Contains(u.Id)).ToHashSet();

            ThrowExceptionIfUsersNotFound(usersSet);

            usersSet.Add(admin);

            Chat chat = new Chat()
            {
                Admin = admin,
                Type = chatTypeObj
            };

            switch (chatTypeObj.Id)
            {
                case 1:

                    if (usersSet.Count != 2)
                    {
                        throw ServerExceptionFactory.InvalidP2PChatUsersCount();
                    }

                    //ThrowExceptionIfP2PChatExists(usersSet);
                    Chat? checkP2PChat = _context.Chats.Include(c => c.Type)
                                      .Include(c => c.Users)
                                      .FirstOrDefault(c => c.Type.Id == (long)ChatTypes.P2P
                                                        && c.Users.Contains(usersSet.First())
                                                        && c.Users.Contains(usersSet.Last()));

                    if (null != checkP2PChat)
                    {
                        return GetChatById(userId, checkP2PChat.Id);
                    }

                    Chat p2pChat = CreateP2PChat(chat, usersSet);
                    NotifyAllConectedUsersToNewChatAsync(userId, p2pChat.Id);
                    return p2pChat;
                case 2:
                    return CreateGroupChat(chat, usersSet, request);
                case 3:
                    return CreateChannel(chat, usersSet, request);
            }

            throw new NotImplementedException();
        }

        private void ThrowExceptionIfP2PChatExists(ICollection<User> usersSet)
        {
            if (null != _context.Chats.Include(c => c.Type)
                                      .Include(c => c.Users)
                                      .FirstOrDefault(c => c.Type.Id == (long)ChatTypes.P2P
                                                        && c.Users.Contains(usersSet.First())
                                                        && c.Users.Contains(usersSet.Last())))
            {
                throw ServerExceptionFactory.ChatAlreadyExists();
            }
        }

        private Chat CreateChannel(Chat chat, HashSet<User> users,
            ChatCreateRequest request)
        {
            _logger.LogDebug("Creating channel with admin id: {AdminId}", chat.Admin.Id);

            chat.MakeChanelChat(request);

            foreach (User user in users)
            {
                chat.AddUser(user);
            }

            _context.Chats.Add(chat);
            _context.SaveChanges();

            if (request.Image != null)
            {
                chat.Image = _mediaService.UploadFile(request.Image, chat);
            }

            _context.SaveChanges();

            return chat;
        }

        private Chat CreateGroupChat(Chat chat, HashSet<User> users,
            ChatCreateRequest request)
        {
            _logger.LogDebug("Creating group with admin id: {AdminId}", chat.Admin.Id);

            chat.MakeGroupChat(request);

            foreach (User user in users)
            {
                chat.AddUser(user);
            }

            _context.Chats.Add(chat);
            _context.SaveChanges();

            if (request.Image != null)
            {
                chat.Image = _mediaService.UploadFile(request.Image, chat);
            }

            _context.SaveChanges();

            return chat;
        }

        private Chat CreateP2PChat(Chat chat, HashSet<User> users)
        {
            _logger.LogDebug("Creating p2p chat with admin id: {AdminId}", chat.Admin.Id);

            foreach (User user in users)
            {
                chat.AddUser(user);
            }

            _context.Chats.Add(chat);
            _context.SaveChanges();
            return chat;
        }

        public Chat GetChatById(long userId, long chatId)
        {
            _logger.LogDebug("Getting chat with id: {ChatId}", chatId);
            User user = _userService.GetUserById(userId);

            Chat chat = GetChat(chatId);

            if (!chat.IsUserInChat(user))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            return chat;
        }

        public ICollection<Chat> GetChatsByName(long userId, string name)
        {
            _logger.LogDebug("Getting chats with name: {Name}", name);
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
            _logger.LogDebug("Getting chats with tag: {Tag}", tag);
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
            _logger.LogDebug("Getting chats with user id: {UserId}", userId);
            User user = _userService.GetUserById(userId);
            HashSet<Chat> userChats = _context.Chats
                                                .Include(c => c.Type)
                                                .Include(c => c.Users)
                                                .ThenInclude(u => u.Image)
                                                .Include(c => c.Image)
                                                .Where(c => c.Users.Contains(user))
                                              .ToHashSet();
            return userChats;
        }

        public Chat UpdateChatInfo(long userId, long chatId, ChatUpdateRequest request)
        {
            _logger.LogDebug("Updating chat with id: {ChatId}", chatId);
            User user = _userService.GetUserById(userId);

            Chat chat = GetChat(chatId);

            if (chat.IsChatP2P())
            {
                throw ServerExceptionFactory.ChatCanNotBeUpdated();
            }

            if (!chat.IsUserAdmin(user))
            {
                throw ServerExceptionFactory.UserNotAdmin();
            }

            if (request.Tag != null && IsTagTaken(request.Tag))
            {
                throw ServerExceptionFactory.UniqueNameIsTaken(request.Tag);
            }

            chat.UpdateFieldsIfExists(request);

            if (request.Image != null)
            {
                if (chat.Image != null)
                {
                    _mediaService.DeleteFile(chat.Image);
                }
                chat.Image = _mediaService.UploadFile(request.Image, chat);
            }

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return chat;
        }

        public ICollection<Chat> GetChats(long userId, string? name, string? tag)
        {
            _logger.LogDebug("Getting chats with user id: {UserId}", userId);

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
            _logger.LogDebug("Joining chat with id: {ChatId}", chatId);
            User user = _userService.GetUserById(userId);

            Chat chat = GetChat(chatId);

            if (chat.IsUserInChat(user))
            {
                throw ServerExceptionFactory.UserAlreadyInChat();
            }

            if (!chat.IsChatPublic())
            {
                throw ServerExceptionFactory.ChatNotPublic();
            }

            chat.AddUser(user);

            _context.Chats.Update(chat);
            _context.SaveChanges();

            return "Joined successfully";
        }

        public string LeaveChat(long userId, long chatId)
        {
            _logger.LogDebug("Leaving chat with id: {ChatId}", chatId);
            User user = _userService.GetUserById(userId);
            Chat chat = GetChat(chatId);

            if (!chat.IsUserInChat(user))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }

            if (chat.IsChatP2P())
            {
                chat.RemoveUser(user);
                if (chat.IsChatEmpty())
                {
                    if (chat.Image != null)
                    {
                        _mediaService.DeleteFile(chat.Image);
                    }
                    _context.Chats.Remove(chat);
                }
                else
                {
                    chat.MakeLastUserAdmin();
                    _context.Chats.Update(chat);
                }
            }
            else
            {
                if (chat.IsChatWithLastUser())
                {
                    chat.RemoveUser(user);
                    if (chat.Image != null)
                    {
                        _mediaService.DeleteFile(chat.Image);
                    }
                    _context.Chats.Remove(chat);
                }
                else if (chat.IsUserAdmin(user))
                {
                    throw ServerExceptionFactory.UserCanNotLeaveChatAsAdmin();
                }
                chat.RemoveUser(user);
                _context.Chats.Update(chat);
            }

            _context.SaveChanges();

            return "Left successfully";
        }

        private async Task NotifyAllConectedUsersToNewChatAsync(long userId, long chatId, string type = "NewChat")
        {
            Chat chat = GetChatById(userId, chatId);
            _logger.LogDebug("Notify all conected users by chat id: {ChatId}", chat.Id);

            HashSet<long> usersIds = new HashSet<long>(chat.Users.Select(u => u.Id));

            List<string> connectionIds = new List<string>();

            foreach (long userIdToNotify in usersIds)
            {
                if (ChatHub.UserConnections.TryGetValue(userIdToNotify, out var connections))
                {
                    lock (connections)
                    {
                        connectionIds.AddRange(connections);
                    }
                }
            }

            foreach (string connectionId in connectionIds)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(type, chat);
            }
        }
    }
}
