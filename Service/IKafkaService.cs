using ASP_Chat.Service.Requests;

namespace ASP_Chat.Service
{
    public interface IKafkaService
    {
        Task SendMessageAsync(FileRequest fileRequest);
        Task<string> WaitForResponseAsync(string key, string responseTopic);
    }
}
