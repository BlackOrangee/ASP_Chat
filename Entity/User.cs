namespace ASP_Chat.Entity
{
    public class User
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Description { get; set; }
        public Media? Image { get; set; }
        public ICollection<Chat> AdminedChats { get; set; } = new HashSet<Chat>();
        public ICollection<Chat> ModeratedChats { get; set; } = new HashSet<Chat>();
        public ICollection<Chat> Chats { get; set; } = new HashSet<Chat>();
        public ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}
