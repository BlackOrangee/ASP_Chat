using ASP_Chat.Entity;
using ASP_Chat.Exception;
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

            if ((message.Chat.Type.Id == (long)EChatType.P2P && message.User.Id != userId) 
                || (message.Chat.Moderators != null && !message.Chat.Moderators.Contains(user)))
            {
                throw new CustomException("You have no permission to delete this message",
                CustomException.ExceptionCodes.NoPermissionToDeleteMessage,
                CustomException.StatusCodes.BadRequest);
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

            if ((message.Chat.Type.Id == (long)EChatType.P2P && message.User.Id != userId)
               || (message.Chat.Moderators != null && !message.Chat.Moderators.Contains(user)))
            {
                throw new CustomException("You have no permission to edit this message",
                CustomException.ExceptionCodes.NoPermissionToEditMessage,
                CustomException.StatusCodes.BadRequest);
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

            if (chat.Type.Id == (long)EChatType.Channel 
                && ((chat.Moderators != null && !chat.Moderators.Contains(user)) || !chat.Admin.Id.Equals(userId)))
            {
                throw new CustomException("You have no permission to send messages in this chat",
                CustomException.ExceptionCodes.NoPermissionToSendMessage,
                CustomException.StatusCodes.BadRequest);
            }

            if(text == null && file == null)
            {
                throw new CustomException("Message is empty", 
                CustomException.ExceptionCodes.MessageIsEmpty,
                CustomException.StatusCodes.BadRequest);
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
                Message replyMessage = GetMessage(userId, replyMessageId.Value);
                message.ReplyMessage = replyMessage;
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
                throw new CustomException("Message not found",
                CustomException.ExceptionCodes.MessageNotFound,
                CustomException.StatusCodes.NotFound);
            }

            Chat chat = _chatService.GetChatById(userId, message.Chat.Id);

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

            HashSet<Message> messages = chat.Messages.ToHashSet();

            if (lastMessageId != null)
            {
                messages = chat.Messages.Where(m => m.Id > lastMessageId).ToHashSet();
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
