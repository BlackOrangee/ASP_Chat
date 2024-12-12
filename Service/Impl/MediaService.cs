using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

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

        public Media UploadFile(IFormFile fileData, object holder)
        {
            _logger.LogDebug("Uploading file");

            byte[] fileDataBytes = GetFileBytesAsync(fileData).GetAwaiter().GetResult();

            string className = holder.GetType().Name;
            var idProperty = holder.GetType().GetProperty("Id");
            var idValue = idProperty != null ? idProperty.GetValue(holder)?.ToString() : "0";
            string hash = GenerateHash(fileDataBytes);
            string fileExtension = Path.GetExtension(fileData.FileName);
            string uniqueFileName = $"{hash}{fileExtension}";

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

        public string GetFileLink(long mediaId, long userId)
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

            Task.Run(async () =>
            {
                try
                {
                    await _kafkaService.SendMessageAsync(new FileRequest()
                    {
                        Operation = "Get",
                        FileName = media.Url
                    });
                }
                catch (Exception ex)
                {
                    ServerExceptionFactory.KafkaException(ex.Message);
                }
            });

            return _kafkaService.WaitForResponseAsync(media.Url, "media-responses")
                                        .GetAwaiter()
                                        .GetResult();
        }

        private bool IsUserCanSeeFile(Media media, User user)
        {
            if (media.Users != null && media.Users.Count > 0)
            {
                return true;
            }

            if (media.Chats != null && media.Chats.Count > 0)
            {
                foreach (Chat chat in media.Chats)
                {
                    Chat? loadedChat = _context.Chats.Include(c => c.Users)
                                                     .Include(c => c.Type)
                                                     .FirstOrDefault(c => c.Id == chat.Id);
                    
                    if ( loadedChat != null && ( loadedChat.IsChatPublic() || loadedChat.IsUserInChat(user) ) )
                    {
                        return true;
                    }
                }
            }

            if (media.Messages != null && media.Messages.Count > 0)
            {
                foreach (Message message in media.Messages)
                {
                    Message? loadedMessage = _context.Messages.Include(m => m.User)
                                                              .Include(m => m.Chat)
                                                              .ThenInclude(c => c.Users)
                                                              .FirstOrDefault(m => m.Id == message.Id);
                    
                    if (loadedMessage != null && loadedMessage.Chat.Users.Contains(user))
                    {
                        return true;
                    }
                }
            }
            return false;
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
