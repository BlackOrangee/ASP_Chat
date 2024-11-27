using Newtonsoft.Json;

namespace ASP_Chat.Entity
{
    public class Chat
    {
        public long Id { get; set; }
        public long AdminId { get; set; }
        public ChatType Type { get; set; }
        public User Admin { get; set; }
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Media? Image { get; set; }
        public ICollection<User> Users { get; set; } = new HashSet<User>();
        public ICollection<Message>? Messages { get; set; } = new HashSet<Message>();
        public ICollection<User>? Moderators { get; set; } = new HashSet<User>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
