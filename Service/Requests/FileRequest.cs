namespace ASP_Chat.Service.Requests
{
    public class FileRequest
    {
        public string Operation { get; set; }
        public string FileName { get; set; }
        public string? FileData { get; set; }
    }
}
