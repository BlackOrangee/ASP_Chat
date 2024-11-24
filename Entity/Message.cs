using Newtonsoft.Json;

namespace ASP_Chat.Entity
{
    public class Message
    {
        public long Id { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }
        public Message? ReplyMessage { get; set; }
        public DateTime Date { get; set; }
        public string? Text { get; set; }
        public bool IsEdited { get; set; } = false;
        public bool IsReaded { get; set; } = false;
        public ICollection<Media>? Media { get; set; } = new HashSet<Media>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
