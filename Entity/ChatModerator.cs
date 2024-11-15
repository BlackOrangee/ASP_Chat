namespace ASP_Chat.Entity
{
    public class ChatModerator
    {
        public long Id { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }
    }
}
