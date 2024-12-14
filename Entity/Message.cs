using System.Text.Json.Serialization;
using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Entity
{
    public class Message
    {
        public long Id { get; set; }

        public User User { get; set; }

        [JsonIgnore]
        public Chat Chat { get; set; }

        public Message? ReplyMessage { get; set; }

        public DateTime Date { get; set; }

        public string? Text { get; set; }

        public bool IsEdited { get; set; } = false;

        public bool IsReaded { get; set; } = false;

        public ICollection<Media>? Media { get; set; } = new HashSet<Media>();

        public bool IsUserSender(User user)
        {
            return User.Id == user.Id;
        }

        public bool IsUserHavePermissionToModifyMessage(User user)
        {
            if ((Chat.IsChatP2P() && IsUserSender(user))
                || Chat.IsUserModerator(user)
                || Chat.IsUserAdmin(user))
            {
                return true;
            }

            return false;
        }

        public void Edit(string? text)
        {
            Text = text;
            IsEdited = true;
        }

        public void AddTextIfExists(MessageSendRequest request)
        {
            if (request.Text != null)
            {
                Text = request.Text;
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
