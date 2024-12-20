using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IMediaService
    {
        Media UploadFile<T>(IFormFile fileData, T holder) where T : class, IEntityWithId;
        string DeleteFile(Media media);
        Task<string> GetFileLink(long mediaId, long userId, int timeToLive);
    }
}
