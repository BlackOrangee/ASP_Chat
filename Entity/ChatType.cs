using System.Text.Json.Serialization;

namespace ASP_Chat.Entity
{
    public class ChatType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Chat>? Chats { get; set; } = new HashSet<Chat>();
    }
}
