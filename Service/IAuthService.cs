namespace ASP_Chat.Service
{
    public interface IAuthService
    {
        string Login(string username, string password);
        string Register(string username, string password);
        string ChangePassword(long userId, string oldPassword, string newPassword);
    }
}
