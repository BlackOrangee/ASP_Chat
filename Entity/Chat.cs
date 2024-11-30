using ASP_Chat.Exceptions;
using System;
using Newtonsoft.Json;
using ASP_Chat.Enums;
using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Entity
{
    public class Chat
    {
        public long Id { get; set; }
        public long AdminId { get; set; }
        public ChatType Type { get; set; }
        public User Admin { get; set; }
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Media? Image { get; set; }
        public ICollection<User> Users { get; set; } = new HashSet<User>();
        public ICollection<Message>? Messages { get; set; } = new HashSet<Message>();
        public ICollection<User>? Moderators { get; set; } = new HashSet<User>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

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
            return Moderators != null && Moderators.Contains(user);
        }

        public bool IsUserInChat(User user)
        {
            return Users.Contains(user);
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

        public void UpdateFieldsIfExists(ChatRequest request)
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

        public void MakeChanelChat(ChatRequest request, Media? image)
        {
            if (string.IsNullOrEmpty(request.Description)) 
            {
                request.Description = "Channel description";
            } 

            Tag = request.Tag;
            Name = request.Name;
            Description = request.Description;
            Image = image;
        }

        public void MakeGroupChat(ChatRequest request, Media? image)
        {
            if (string.IsNullOrEmpty(request.Description))
            {
                request.Description = "Group description";
            }

            Name = request.Name;
            Description = request.Description;
            Image = image;
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
