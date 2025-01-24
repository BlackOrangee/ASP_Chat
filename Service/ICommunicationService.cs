using ASP_Chat.Service.Requests;

namespace ASP_Chat.Service
{
    public interface ICommunicationService
    {
        void SendMessage(FileRequest fileRequest);
        string WaitForResponse(string key);
    }
}
