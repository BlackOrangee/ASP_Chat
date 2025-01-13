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
        public DbSet<ChatType> ChatTypes { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MediaRequest> MediaRequests { get; set; }
        public DbSet<MediaResponse> MediaResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.AdminedChats)
                .HasForeignKey(c => c.AdminId);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Chats);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Moderators)
                .WithMany(u => u.ModeratedChats);
        }
    }
}
