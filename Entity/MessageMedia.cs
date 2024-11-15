namespace ASP_Chat.Entity
{
    public class MessageMedia
    {
        public long Id { get; set; }
        public Message Message { get; set; }
        public Media Media { get; set; }
    }
}
