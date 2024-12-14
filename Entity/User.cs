using System.Text.Json.Serialization;
using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Entity
{
    public class User : IEntityWithId
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string? Description { get; set; }
        public Media? Image { get; set; }
        [JsonIgnore]
        public ICollection<Chat> AdminedChats { get; set; } = new HashSet<Chat>();
        [JsonIgnore]
        public ICollection<Chat> ModeratedChats { get; set; } = new HashSet<Chat>();
        [JsonIgnore]
        public ICollection<Chat> Chats { get; set; } = new HashSet<Chat>();
        [JsonIgnore]
        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

        public void UpdateFieldsIfExists(UserUpdateRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                Name = request.Name;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                Description = request.Description;
            }
        }
    }
}
