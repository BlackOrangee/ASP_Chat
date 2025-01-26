using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Hubs;
using Autofac;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ASP_Chat.Service.Impl
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IMediaService _mediaService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageService(ApplicationDBContext context,
                              ILogger<MessageService> logger,
                              IUserService userService,
                              IChatService chatService,
                              IMediaService mediaService,
                              IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
            _chatService = chatService;
            _mediaService = mediaService;
            _hubContext = hubContext;
        }

        public string DeleteMessage(long userId, long messageId)
        {
            _logger.LogDebug("Deleting message with id: {MessageId}", messageId);
            Message message = GetMessage(userId, messageId);
            User user = _userService.GetUserById(userId);

            if (!message.IsUserHavePermissionToModifyMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToDeleteMessage();
            }

            if (message.Media != null)
            {
                foreach (Media m in message.Media)
                {
                    _mediaService.DeleteFile(m);
                }
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return "Message deleted successfully";
        }

        public Message EditMessage(long userId, long messageId, MessageEditRequest request)
        {
            _logger.LogDebug("Editing message with id: {MessageId}", messageId);
            Message message = GetMessage(userId, messageId);
            User user = _userService.GetUserById(userId);

            if (!message.IsUserHavePermissionToModifyMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToEditMessage();
            }

            message.Edit(request.Text);

            _context.Messages.Update(message);
            _context.SaveChanges();

            return message;
        }

        public Message SendMessage(long userId, MessageSendRequest request)
        {
            _logger.LogDebug("Sending message to chat with id: {ChatId}", request.ChatId);
            Chat chat = _chatService.GetChatById(userId, request.ChatId);
            User user = _userService.GetUserById(userId);

            if (!chat.IsUserHavePermissionToSendMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToSendMessage();
            }

            Message message = new Message()
            {
                User = user,
                Chat = chat,
                Date = DateTime.UtcNow,
                Media = new HashSet<Media>()
            };

            if (request.File != null)
            {
                Media media = _mediaService.UploadFile(request.File, chat);
                message.Media.Add(media);
            }

            message.AddTextIfExists(request);

            if (request.ReplyMessageId != null)
            {
                message.ReplyMessage = GetMessage(userId, request.ReplyMessageId.Value);
            }

            message.AddToChat(chat);

            _context.Messages.Add(message);
            _context.SaveChanges();
            return message;
        }

        public Message GetMessage(long userId, long messageId)
        {
            _logger.LogDebug("Getting message with id: {MessageId}", messageId);
            Message? message = _context.Messages.Include(m => m.User)
                                                .Include(m => m.Media)
                                                .Include(m => m.Chat)
                                                .ThenInclude(c => c.Type)
                                                .FirstOrDefault(m => m.Id == messageId);

            if (message == null)
            {
                throw ServerExceptionFactory.MessageNotFound();
            }

            _chatService.GetChatById(userId, message.Chat.Id);

            return message;
        }

        public ICollection<Message> GetMessages(long userId, long chatId, long? lastMessageId)
        {
            _logger.LogDebug("Getting messages with chat id: {ChatId}", chatId);

            Chat chat = _chatService.GetChatById(userId, chatId);

            return chat.GetMessages(lastMessageId);
        }

        public Message SetReadedMessageStatus(long userId, long messageId)
        {
            _logger.LogDebug("Setting message with id: {MessageId} as readed", messageId);
            Message message = GetMessage(userId, messageId);

            if (message.User.Id != userId)
            {
                message.SetReaded();
                _context.Messages.Update(message);
                _context.SaveChanges();
            }

            return message;
        }

        public async Task<Message> AttachMediaToMessage(long userId, MessageAttachMediaRequest mediaAttachRequest)
        {
            _logger.LogDebug("Attaching media to message with id: {MessageId}", mediaAttachRequest.MessageId);
            User user = _userService.GetUserById(userId);
            Message message = GetMessage(userId, mediaAttachRequest.MessageId);

            if (!message.IsUserSender(user))
            {
                throw ServerExceptionFactory.NoPermissionToEditMessage();
            }

            Media media = _mediaService.UploadFile(mediaAttachRequest.File, message.Chat);

            if (message.Media == null)
            {
                message.Media = new HashSet<Media>();
            }

            message.Media.Add(media);
            _context.Messages.Update(message);
            _context.SaveChanges();

            await NotifyAllConectedUsersByChatIdAsync(user.Id, message.Chat.Id, message, "AttachMediaToMessage");

            return message;
        }

        private async Task NotifyAllConectedUsersByChatIdAsync(long userId, long chatId, Message message, string type = "ReceiveMessage")
        {
            Chat chat = _chatService.GetChatById(userId, chatId);
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
                await _hubContext.Clients.Client(connectionId).SendAsync(type, message);
            }
        }
    }
}
