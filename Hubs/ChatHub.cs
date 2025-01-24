using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ASP_Chat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<long, string> _connections = new();

        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, IChatService chatService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug("OnConnectedAsync triggered");
            _connections[GetUserIdFromContextToken()] = Context.ConnectionId;
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connections.TryRemove(GetUserIdFromContextToken(), out _);
            await base.OnDisconnectedAsync(exception);
        }

        private long GetUserIdFromContextToken()
        {
            if ( Context.User != null 
                && Context.User.Identity != null 
                && Context.User.Identity.IsAuthenticated == true )
            {
                Claim? subClaim = Context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub 
                                                                        || c.Type == ClaimTypes.NameIdentifier);
                if (subClaim != null)
                {
                    return long.Parse(subClaim.Value);
                }
            }
            throw ServerExceptionFactory.InvalidToken();
        }

        private async Task NotifyAllConectedUsersByChatIdAsync(long userId, long chatId, Message message, string type = "ReceiveMessage")
        {
            Chat chat = _chatService.GetChatById(userId, chatId);
            _logger.LogDebug("Notify all conected users by chat id: {ChatId}", chat.Id);

            HashSet<long> usersIds = new HashSet<long>(chat.Users.Select(u => u.Id));

            foreach (long connectionId in _connections.Keys)
            {
                if (usersIds.Contains(connectionId))
                {
                    await Clients.Client(_connections[connectionId]).SendAsync(type, message);
                }
            }
        }

        public async Task SendMessageToChatAsync(MessageSendRequest request)
        {
            long userId = GetUserIdFromContextToken();

            _logger.LogInformation("User with {Id} is sending a message to chat {ChatId}", userId, request.ChatId);

            Message message = _messageService.SendMessage(userId, request);

            await NotifyAllConectedUsersByChatIdAsync(userId, request.ChatId, message);
        }

        public async Task SetReadedMessageStatusAsync(long messageId)
        {
            long userId = GetUserIdFromContextToken();

            _logger.LogInformation("User with {Id} set readed message with id: {MessageId}", userId, messageId);

            Message message = _messageService.SetReadedMessageStatus(userId, messageId);

            await NotifyAllConectedUsersByChatIdAsync(userId, message.Chat.Id, message, "ReadedMessage");
        }
    }
}
