using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Service
{
    public interface IAuthService
    {
        string Login(AuthRequest request);
        string Register(AuthRequest request);
        string ChangePassword(AuthRequest userRequest);
    }
}
