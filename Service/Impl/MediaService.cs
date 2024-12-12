using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;
using System.Security.Cryptography;

namespace ASP_Chat.Service.Impl
{
    public class MediaService : IMediaService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<MediaService> _logger;
        private readonly IKafkaService _kafkaProducerService;

        public MediaService(ApplicationDBContext context,
                            ILogger<MediaService> logger,
                            IKafkaService kafkaProducerService)
        {
            _context = context;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
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
                    await _kafkaProducerService.SendMessageAsync(new FileRequest()
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

        public string GetFileLink(Media media)
        {
            _logger.LogDebug("Getting file link");
            Task.Run(async () =>
            {
                try
                {
                    await _kafkaProducerService.SendMessageAsync(new FileRequest()
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

            return _kafkaProducerService.WaitForResponseAsync(media.Url, "media-responses")
                                        .GetAwaiter()
                                        .GetResult();
        }

        public string DeleteFile(Media media)
        {
            _logger.LogDebug("Deleting file");
            Task.Run(async () =>
            {
                try
                {
                    await _kafkaProducerService.SendMessageAsync(new FileRequest()
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
