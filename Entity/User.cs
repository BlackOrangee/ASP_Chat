using ASP_Chat.Controllers.Request;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace ASP_Chat.Entity
{
    public class User
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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public void UpdateUsernameIfExists(UserRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                Username = request.Username;
            }
        }

        public void UpdateNameIfExists(UserRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                Name = request.Name;
            }
        }

        public void UpdateDescriptionIfExists(UserRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                Description = request.Description;
            }
        }
    }
}
