namespace ASP_Chat.Entity
{
    public class Media
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public ICollection<Chat>? Chats { get; set; } = new HashSet<Chat>();
        public ICollection<User>? Users { get; set; } = new HashSet<User>();
        public ICollection<Message>? Messages { get; set; } = new HashSet<Message>();
    }
}
