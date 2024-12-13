using System.Text.Json.Serialization;

namespace ASP_Chat.Entity
{
    public class Media
    {
        public long Id { get; set; }

        [JsonIgnore]
        public string Url { get; set; }

        [JsonIgnore]
        public ICollection<Chat>? Chats { get; set; } = new HashSet<Chat>();

        [JsonIgnore]
        public ICollection<User>? Users { get; set; } = new HashSet<User>();

        [JsonIgnore]
        public ICollection<Message>? Messages { get; set; } = new HashSet<Message>();
    }
}
