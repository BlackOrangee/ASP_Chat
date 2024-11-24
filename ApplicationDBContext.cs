using ASP_Chat.Entity;
using Microsoft.EntityFrameworkCore;

namespace ASP_Chat
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<Chat> Chats { get; set; }
        //public DbSet<ChatModerator> ChatModerators { get; set; }
        public DbSet<ChatType> ChatTypes { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Message> Messages { get; set; }
        //public DbSet<MessageMedia> MessageMedias { get; set; }
        public DbSet<User> Users { get; set; }
        //public DbSet<UserChat> UserChats { get; set; }
    }
}
