namespace ASP_Chat.Entity
{
    public class UserChat
    {
        public long id {get; set;}
        public User User {get; set;}
        public Chat Chat {get; set;}
    }
}
