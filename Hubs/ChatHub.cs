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
        public static readonly ConcurrentDictionary<long, HashSet<string>> UserConnections = new();

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
            long userId = GetUserIdFromContextToken();

            var connections = UserConnections.GetOrAdd(userId, _ => new HashSet<string>());
            lock (connections)
            {
                connections.Add(Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            long userId = GetUserIdFromContextToken();

            if (UserConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(Context.ConnectionId);
                    if (!connections.Any())
                    {
                        UserConnections.TryRemove(userId, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private long GetUserIdFromContextToken()
        {
            if (Context.User != null
                && Context.User.Identity != null
                && Context.User.Identity.IsAuthenticated)
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

            List<string> connectionIds = new List<string>();

            foreach (long userIdToNotify in usersIds)
            {
                if (UserConnections.TryGetValue(userIdToNotify, out var connections))
                {
                    lock (connections)
                    {
                        connectionIds.AddRange(connections);
                    }
                }
            }

            foreach (string connectionId in connectionIds)
            {
                await Clients.Client(connectionId).SendAsync(type, message, new { ChatId = chat.Id });
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
