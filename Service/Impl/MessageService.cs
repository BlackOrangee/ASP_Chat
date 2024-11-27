using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Enums;

namespace ASP_Chat.Service.Impl
{
    public class MessageService : IMessageService
    {
        private ApplicationDBContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public MessageService(ApplicationDBContext context, ILogger<MessageService> logger, 
            IUserService userService, IChatService chatService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
            _chatService = chatService;
        }

        public string DeleteMessage(long userId, long messageId)
        {
            _logger.LogDebug("Deleting message with id: {messageId}", messageId);
            Message message = GetMessage(userId, messageId);
            User user = _userService.GetUserById(userId);

            if ((message.Chat.Type.Id == (long)ChatTypes.P2P && message.User.Id != userId) 
                || (message.Chat.Moderators != null && !message.Chat.Moderators.Contains(user)))
            {
                throw ServerExceptionFactory.NoPermissionToDeleteMessage();
            }
            _context.Messages.Remove(message);
            _context.SaveChanges();

            return "Message deleted successfully";
        }

        public Message EditMessage(long userId, long messageId, string text)
        {
            _logger.LogDebug("Editing message with id: {messageId}", messageId);
            Message message = GetMessage(userId, messageId);
            User user = _userService.GetUserById(userId);

            if ((message.Chat.Type.Id == (long)ChatTypes.P2P && message.User.Id != userId)
               || (message.Chat.Moderators != null && !message.Chat.Moderators.Contains(user)))
            {
                throw ServerExceptionFactory.NoPermissionToEditMessage();
            }
            message.Text = text;
            message.IsEdited = true;
            _context.Messages.Update(message);
            _context.SaveChanges();
            return message;
        }

        public Message SendMessage(long userId, long chatId, long? replyMessageId, string? text, ICollection<byte[]>? file)
        {
            _logger.LogDebug("Sending message to chat with id: {chatId}", chatId);
            Chat chat = _chatService.GetChatById(userId, chatId);

            User user = _userService.GetUserById(userId);

            if (chat.Type.Id == (long)ChatTypes.Channel 
                && ((chat.Moderators != null && !chat.Moderators.Contains(user)) || !chat.Admin.Id.Equals(userId)))
            {
                throw ServerExceptionFactory.NoPermissionToSendMessage();
            }

            if(text == null && file == null)
            {
                throw ServerExceptionFactory.MessageIsEmpty();
            }

            Message message = new Message() 
            { 
                User = user,
                Chat = chat,
                Date = DateTime.Now
            };

            if(text != null)
            {
                message.Text = text;
            }

            if(file != null)
            {
                //TODO: file upload
                message.Media = new HashSet<Media>();
            }

            if (replyMessageId != null)
            {
                message.ReplyMessage = GetMessage(userId, replyMessageId.Value);
            }

            if (chat.Messages == null || chat.Messages.Count == 0)
            {
                chat.Messages = new HashSet<Message>();
            }

            chat.Messages.Add(message);
            _context.Messages.Add(message);
            _context.SaveChanges();

            return message;
        }

        public Message GetMessage(long userId, long messageId)
        {
            _logger.LogDebug("Getting message with id: {messageId}", messageId);
            Message? message = _context.Messages.FirstOrDefault(m => m.Id == messageId);

            if (message == null)
            {
                throw ServerExceptionFactory.MessageNotFound();
            }

            _chatService.GetChatById(userId, message.Chat.Id);

            return message;
        }

        public ICollection<Message> GetMessages(long userId, long chatId, long? lastMessageId)
        {
            _logger.LogDebug("Getting messages with chat id: {chatId}", chatId);

            Chat chat = _chatService.GetChatById(userId, chatId);

            if(chat.Messages == null || chat.Messages.Count == 0)
            {
                chat.Messages = new HashSet<Message>();
            }

            HashSet<Message> messages = new HashSet<Message>();

            if (lastMessageId != null)
            {
                messages = chat.Messages.Where(m => m.Id > lastMessageId).ToHashSet();
            }
            else
            {
                messages = chat.Messages.ToHashSet();
            }

            return messages;
        }

        public void SetReadedMessageStatus(long userId, long messageId)
        {
            _logger.LogDebug("Setting message with id: {messageId} as readed", messageId);
            Message message = GetMessage(userId, messageId);
            message.IsReaded = true;
            _context.Messages.Update(message);
            _context.SaveChanges();
        }
    }
}
