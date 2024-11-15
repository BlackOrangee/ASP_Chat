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
    }
}
