using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IMediaService
    {
        Media UploadFile(IFormFile fileData, object holder);
        string DeleteFile(Media media);
        string GetFileLink(Media media);
    }
}
