namespace ASP_Chat.Entity
{
    public class Chat
    {
        public long Id { get; set; }
        public ChatType Type { get; set; }
        public User? Admin { get; set; }
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Media? Image { get; set; }
    }
}
