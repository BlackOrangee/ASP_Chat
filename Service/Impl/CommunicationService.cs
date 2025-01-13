using System.Text.Json;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;

namespace ASP_Chat.Service.Impl
{
    public class CommunicationService : ICommunicationService
    {
        private readonly ILogger<CommunicationService> _logger;
        private readonly ApplicationDBContext _context;

        public CommunicationService(ILogger<CommunicationService> logger, ApplicationDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void SendMessage(FileRequest fileRequest)
        {
            string messageValue = JsonSerializer.Serialize(fileRequest);

            _context.MediaRequests.Add(
                new MediaRequest
                {
                    Key = fileRequest.Operation,
                    Timestamp = DateTime.UtcNow.ToString("o"),
                    Data = messageValue,
                    Locked = false,
                    Done = false
                }
            );

            _context.SaveChanges();
            _logger.LogDebug("Message sent to Media Request with key '{Operation}' and value '{MessageValue}'",
                fileRequest.Operation, messageValue);

        }

        public string WaitForResponse(string key)
        {
            var timeout = TimeSpan.FromSeconds(100);
            var startTime = DateTime.UtcNow;

            _logger.LogDebug("Waiting for response with key '{Key}'", key);

            while (DateTime.UtcNow - startTime < timeout)
            {
                MediaResponse? mediaResponse = _context.MediaResponses.FirstOrDefault(r => r.Key == key && r.Done);

                if (mediaResponse != null)
                {
                    return mediaResponse.Data;
                }

                Thread.Sleep(100);
            }

            throw ServerExceptionFactory.RequestTimeout();
        }
    }
}
