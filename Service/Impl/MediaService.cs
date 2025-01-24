using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ASP_Chat.Service.Impl
{
    public class MediaService : IMediaService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<MediaService> _logger;
        private readonly ICommunicationService _communicationService;

        public MediaService(ApplicationDBContext context,
                            ILogger<MediaService> logger,
                            ICommunicationService communicationService)
        {
            _context = context;
            _logger = logger;
            _communicationService = communicationService;
        }

        public Media UploadFile<T>(IFormFile fileData, T holder) where T : class, IEntityWithId
        {
            _logger.LogDebug("Uploading file");

            byte[] fileDataBytes = GetFileBytesAsync(fileData);

            string className = typeof(T).Name;
            string idValue = holder.Id.ToString();

            string hash = GenerateHash(fileDataBytes);
            string fileExtension = Path.GetExtension(fileData.FileName);
            string uniqueFileName = $"{hash}{DateTime.Now.Ticks}{fileExtension}";

            string path = $"{className}/{idValue}/{uniqueFileName}";

            _communicationService.SendMessage(new FileRequest()
            {
                Operation = "Save",
                FileName = path,
                FileData = Convert.ToBase64String(fileDataBytes)
            });

            return new Media()
            {
                Url = path
            };
        }

        private static byte[] GetFileBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string GenerateHash(byte[] fileData)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(fileData);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public string GetFileLink(long mediaId, long userId, int timeToLive)
        {
            _logger.LogDebug("Getting file link with id: {MediaId}", mediaId);

            Media? media = _context.Medias.Include(m => m.Users)
                                          .Include(m => m.Chats)
                                          .Include(m => m.Messages)
                                          .FirstOrDefault(m => m.Id == mediaId);
            if (media == null)
            {
                throw ServerExceptionFactory.MediaNotFound(mediaId);
            }

            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                throw ServerExceptionFactory.UserNotFound();
            }

            if (!IsUserCanSeeFile(media, user))
            {
                throw ServerExceptionFactory.NoPermissionToGetMediaLink();
            }

            string correlationId = Guid.NewGuid().ToString();

            _communicationService.SendMessage(new FileRequest()
            {
                Operation = "Get",
                FileName = media.Url,
                CorrelationId = correlationId,
                LifeTime = timeToLive
            });

            return _communicationService.WaitForResponse(correlationId);
        }

        private bool IsUserCanSeeFile(Media media, User user)
        {
            string[] mediaPathParts = media.Url.Split("/");
            string className = mediaPathParts[0];
            string id = mediaPathParts[1];

            if (className.Equals("Chat"))
            {
                Chat? chat = _context.Chats.Include(c => c.Type)
                                           .Include(c => c.Users)
                                           .FirstOrDefault(chat => chat.Id == long.Parse(id));

                return chat != null && (chat.IsChatPublic() || chat.IsUserInChat(user));
            }

            return true;
        }

        public string DeleteFile(Media media)
        {
            _logger.LogDebug("Deleting file");

            _communicationService.SendMessage(new FileRequest()
            {
                Operation = "Delete",
                FileName = media.Url
            });

            _context.Medias.Remove(media);
            _context.SaveChanges();

            return "File deleted successfully";
        }
    }
}
