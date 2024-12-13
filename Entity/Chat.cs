using ASP_Chat.Enums;
using ASP_Chat.Controllers.Request;
using System.Text.Json.Serialization;

namespace ASP_Chat.Entity
{
    public class Chat : IEntityWithId
    {
        public long Id { get; set; }
        public long AdminId { get; set; }
        public ChatType Type { get; set; }
        public User Admin { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Tag { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Media? Image { get; set; }
        public ICollection<User> Users { get; set; } = new HashSet<User>();
        public ICollection<Message>? Messages { get; set; } = new HashSet<Message>();
        public ICollection<User>? Moderators { get; set; } = new HashSet<User>();

        public bool IsChatP2P()
        {
            return Type.Id == (long)ChatTypes.P2P;
        }

        public bool IsChatChannel()
        {
            return Type.Id == (long)ChatTypes.Channel;
        }

        public bool IsChatEmpty()
        {
            return Users.Count == 0;
        }

        public bool IsChatWithLastUser()
        {
            return Users.Count == 1;
        }

        public bool IsUserAdmin(User user) 
        { 
            return user.Id == Admin.Id;
        }

        public bool IsUserModerator(User user)
        {
            if (Moderators == null) 
            { 
                return false; 
            }

            foreach (User u in Moderators)
            {
                if (u.Id == user.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsUserInChat(User user)
        {
            foreach (var item in Users)
            {
                Console.WriteLine(item.Id);
            }

            if (Users == null)
            {
                Console.WriteLine("Users is null");
                return false;
            }

            foreach (User u in Users)
            {
                if (u.Id == user.Id)
                {
                    return true;
                }
            }
            Console.WriteLine("User not in chat");
            return false;
        }

        public bool IsChatPublic()
        {
            return Type.Id == (long)ChatTypes.Channel;
        }

        public bool IsUserHavePermissionToSendMessage(User user)
        {
            if (IsChatChannel() && (!IsUserModerator(user) || !IsUserAdmin(user)))
            {
                return false;
            }

            return true;
        }

        public void UpdateFieldsIfExists(ChatUpdateRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Tag) && IsChatChannel())
            {
                Tag = request.Tag;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                Name = request.Name;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                Description = request.Description;
            }
        }

        public void AddModerator(User moderator)
        {
            if (Moderators == null)
            {
                Moderators = new HashSet<User>();
            }

            Moderators.Add(moderator);
        }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            Users.Remove(user);
        }

        public void MakeChanelChat(ChatCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Description)) 
            {
                request.Description = "Channel description";
            }

            Tag = request.Tag;
            Name = request.Name;
            Description = request.Description;
        }

        public void MakeGroupChat(ChatCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Description))
            {
                request.Description = "Group description";
            }

            Name = request.Name;
            Description = request.Description;
        }

        public void MakeLastUserAdmin()
        {
            if (Users.Count == 1)
            {
                Admin = Users.First();
            }
        }

        public ICollection<Message> GetMessages(long? lastMessageId)
        {
            if (Messages == null)
            {
                Messages = new HashSet<Message>();
            }

            if (lastMessageId != null)
            {
                return Messages.Where(m => m.Id > lastMessageId).ToHashSet();
            }
                
            return Messages.ToHashSet();
        }
    }
}
