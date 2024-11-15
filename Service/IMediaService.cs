namespace ASP_Chat.Service
{
    public interface IMediaService
    {
        Task<string> UploadFile(IFormFile file);
        Task<bool> DeleteFile(string fileName);
        Task<string> GetFile(string fileName);
    }
}
