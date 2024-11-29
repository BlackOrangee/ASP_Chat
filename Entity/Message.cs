using System;
using ASP_Chat.Controllers.Request;
using ASP_Chat.Enums;
using ASP_Chat.Exceptions;
using Newtonsoft.Json;

namespace ASP_Chat.Entity
{
    public class Message
    {
        public long Id { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }
        public Message? ReplyMessage { get; set; }
        public DateTime Date { get; set; }
        public string? Text { get; set; }
        public bool IsEdited { get; set; } = false;
        public bool IsReaded { get; set; } = false;
        public ICollection<Media>? Media { get; set; } = new HashSet<Media>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool IsUserSender(User user)
        {
            return User.Id == user.Id;
        }

        public void ThrowIfNotPermissionToDelete(User user)
        {
            if ((Chat.IsChatP2P() && !IsUserSender(user)) || !Chat.IsUserModerator(user) || !Chat.IsUserAdmin(user))
            {
                throw ServerExceptionFactory.NoPermissionToDeleteMessage();
            }
        }

        public void ThrowIfNotPermissionToEdit(User user)
        {
            if ((Chat.IsChatP2P() && !IsUserSender(user)) || !Chat.IsUserModerator(user) || !Chat.IsUserAdmin(user))
            {
                throw ServerExceptionFactory.NoPermissionToEditMessage();
            }
        }

        public void Edit(string? text)
        {
            Text = text;
            IsEdited = true;
        }

        public void AddText(MessageRequest request)
        {
            if (request.Text != null)
            {
                Text = request.Text;
            }
        }

        public void AddFile(MessageRequest request)
        {
            if (request.File != null)
            {
                //TODO: file upload
                Media = new HashSet<Media>();
            }
        }

        public void AddToChat(Chat chat)
        {
            Chat = chat;
        }

        public void SetReaded()
        {
            IsReaded = true;
        }
    }
}
