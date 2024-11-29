using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Enums;
using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Service.Impl
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDBContext _context;
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
            _logger.LogDebug("Deleting message with id: {MessageId}", messageId);
            Message message = GetMessage(userId, messageId);
            User user = _userService.GetUserById(userId);

            if (!message.IsUserHavePermissionToModifyMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToDeleteMessage();
            }
            
            _context.Messages.Remove(message);
            _context.SaveChanges();

            return "Message deleted successfully";
        }

        public Message EditMessage(MessageRequest request)
        {
            request.SendMessageValidation();
            _logger.LogDebug("Editing message with id: {MessageId}", request.MessageId);
            Message message = GetMessage(request.UserId, request.MessageId);
            User user = _userService.GetUserById(request.UserId);

            if (!message.IsUserHavePermissionToModifyMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToEditMessage();
            }

            message.Edit(request.Text);

            _context.Messages.Update(message);
            _context.SaveChanges();

            return message;
        }

        public Message SendMessage(MessageRequest request)
        {
            request.SendMessageValidation();
            _logger.LogDebug("Sending message to chat with id: {ChatId}", request.ChatId);
            Chat chat = _chatService.GetChatById(request.UserId, request.ChatId.Value);
            User user = _userService.GetUserById(request.UserId);

            if (!chat.IsUserHavePermissionToSendMessage(user))
            {
                throw ServerExceptionFactory.NoPermissionToSendMessage();
            }

            Message message = new Message() 
            { 
                User = user,
                Chat = chat,
                Date = DateTime.Now
            };

            message.AddText(request);

            message.AddFile(request);

            if (request.ReplyMessageId != null)
            {
                message.ReplyMessage = GetMessage(request.UserId, request.ReplyMessageId.Value);
            }

            message.AddToChat(chat);

            _context.Messages.Add(message);
            _context.SaveChanges();

            return message;
        }

        public Message GetMessage(long userId, long messageId)
        {
            _logger.LogDebug("Getting message with id: {MessageId}", messageId);
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
            _logger.LogDebug("Getting messages with chat id: {ChatId}", chatId);

            Chat chat = _chatService.GetChatById(userId, chatId);

            return chat.GetMessages(lastMessageId);
        }

        public void SetReadedMessageStatus(long userId, long messageId)
        {
            _logger.LogDebug("Setting message with id: {MessageId} as readed", messageId);
            Message message = GetMessage(userId, messageId);
            message.SetReaded();

            _context.Messages.Update(message);
            _context.SaveChanges();
        }
    }
}
