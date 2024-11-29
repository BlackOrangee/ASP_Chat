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

        public void UpdateAndSave(Chat chat)
        {
            Chats.Update(chat);
            SaveChanges();
        }

        public void UpdateAndSave(User user)
        {
            Users.Update(user);
            SaveChanges();
        }

        public void UpdateAndSave(Message message)
        {
            Messages.Update(message);
            SaveChanges();
        }

        public void AddAndSave(Chat chat)
        {
            Chats.Add(chat);
            SaveChanges();
        }

        public void AddAndSave(User user)
        {
            Users.Add(user);
            SaveChanges();
        }

        public void AddAndSave(Message message)
        {
            Messages.Add(message);
            SaveChanges();
        }

        public void Remove(Chat chat)
        {
            Chats.Remove(chat);
        }

        public void RemoveAndSave(Message message)
        {
            Messages.Remove(message);
            SaveChanges();
        }

        public void RemoveAndSave(User user)
        {
            Users.Remove(user);
            SaveChanges();
        }

        public User? GetUserById(long id) => Users.FirstOrDefault(u => u.Id == id);

        public User? GetUserByUsername(string username) => Users.FirstOrDefault(u => u.Username == username);

        public ICollection<User> GetUsersByIds(ICollection<long>? ids) => [.. Users.Where(u => ids.Contains(u.Id))];

        public ICollection<User> GetUsersByUsername(string username) => Users.Where(u => u.Username.Contains(username)).ToHashSet();

        public Chat? GetChatById(long id) => Chats.FirstOrDefault(c => c.Id == id);

        public ChatType? GetChatTypeById(long id) => ChatTypes.FirstOrDefault(ct => ct.Id == id);

        public Message? GetMessageById(long id) => Messages.FirstOrDefault(m => m.Id == id);
    }
}
