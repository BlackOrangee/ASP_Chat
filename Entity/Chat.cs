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

        public void ThrowIfUserNotAdmin(User user)
        {
            if (Admin != user)
            {
                throw ServerExceptionFactory.UserNotAdmin();
            }
        }

        public void ThrowIfChatCantHaveModerators()
        {
            if (Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotHaveModerators();
            }
        }

        public void ThrowIfChatCantHaveUsers()
        {
            if (Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotHaveUsers();
            }
        }

        public void ThrowIfChatCantBeUpdated()
        {
            if (Type.Id == (long)ChatTypes.P2P)
            {
                throw ServerExceptionFactory.ChatCanNotBeUpdated();
            }
        }

        public void ThrowIfUserNotInChat(User user)
        {
            if (!Users.Contains(user))
            {
                throw ServerExceptionFactory.UserNotInChat();
            }
        }

        public void ThrowIfUserAlreadyInChat(User user)
        {
            if (Users.Contains(user))
            {
                throw ServerExceptionFactory.UserAlreadyInChat();
            }
        }

        public void ThrowIfUserAlreadyModerator(User user)
        {
            if (Moderators == null)
            {
                Moderators = new HashSet<User>();
            }

            if (Moderators.Contains(user))
            {
                throw ServerExceptionFactory.UserAlreadyModerator();
            }
        }

        public void ThrowIfChatNotPublic()
        {
            if (Type.Id == (long)ChatTypes.Channel)
            {
                return;
            }

            throw ServerExceptionFactory.ChatNotPublic();
        }

        public void UpdateTagIfExists(ChatRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Tag) && chat.Type.Id == (long)ChatTypes.Channel)
            {
                Tag = request.Tag;
            }
        }

        public void UpdateNameIfExists(ChatRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                Name = request.Name;
            }
        }

        public void UpdateDescriptionIfExists(ChatRequest request)
        {
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
            Admin = Users.First();
        }
    }
}
