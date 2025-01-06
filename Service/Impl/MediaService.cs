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
        private readonly IKafkaService _kafkaService;

        public MediaService(ApplicationDBContext context,
                            ILogger<MediaService> logger,
                            IKafkaService kafkaProducerService)
        {
            _context = context;
            _logger = logger;
            _kafkaService = kafkaProducerService;
        }

        public Media UploadFile<T>(IFormFile fileData, T holder) where T : class, IEntityWithId
        {
            _logger.LogDebug("Uploading file");

            byte[] fileDataBytes = GetFileBytesAsync(fileData).GetAwaiter().GetResult();

            string className = typeof(T).Name;
            string idValue = holder.Id.ToString();

            string hash = GenerateHash(fileDataBytes);
            string fileExtension = Path.GetExtension(fileData.FileName);
            string uniqueFileName = $"{hash}{DateTime.Now.Ticks}{fileExtension}";

            string path = $"{className}/{idValue}/{uniqueFileName}";

            Task.Run(async () =>
            {
                try
                {
                    await _kafkaService.SendMessageAsync(new FileRequest()
                    {
                        Operation = "Save",
                        FileName = path,
                        FileData = Convert.ToBase64String(fileDataBytes)
                    });
                }
                catch (Exception ex)
                {
                    ServerExceptionFactory.KafkaException(ex.Message);
                }
            });

            return new Media()
            {
                Url = path
            };
        }

        private async Task<byte[]> GetFileBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
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

        public async Task<string> GetFileLink(long mediaId, long userId, int timeToLive)
        {
            _logger.LogDebug("Getting file link with id: {MediaId}", mediaId);

            Media? media = await _context.Medias
                                            .Include(m => m.Users)
                                            .Include(m => m.Chats)
                                            .Include(m => m.Messages)
                                            .FirstOrDefaultAsync(m => m.Id == mediaId);
            if (media == null)
            {
                throw ServerExceptionFactory.MediaNotFound(mediaId);
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw ServerExceptionFactory.UserNotFound();
            }

            if (!IsUserCanSeeFile(media, user))
            {
                throw ServerExceptionFactory.NoPermissionToGetMediaLink();
            }

            await Task.Run(async () =>
            {
                try
                {
                    await _kafkaService.SendMessageAsync(new FileRequest()
                    {
                        Operation = "Get",
                        FileName = media.Url,
                        LifeTime = timeToLive
                    });
                }
                catch (Exception ex)
                {
                    ServerExceptionFactory.KafkaException(ex.Message);
                }
            });

            return await _kafkaService.WaitForResponseAsync(media.Url, "media-responses");
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
            Task.Run(async () =>
            {
                try
                {
                    await _kafkaService.SendMessageAsync(new FileRequest()
                    {
                        Operation = "Delete",
                        FileName = media.Url
                    });
                }
                catch (Exception ex)
                {
                    ServerExceptionFactory.KafkaException(ex.Message);
                }
            });

            _context.Medias.Remove(media);
            _context.SaveChanges();

            return "File deleted successfully";
        }
    }
}
