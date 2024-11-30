using ASP_Chat.Controllers.Request;

namespace ASP_Chat.Service
{
    public interface IAuthService
    {
        string Login(AuthLoginRequest request);
        string Register(AuthRegisterRequest request);
        string ChangePassword(long id, AuthChangePasswordRequest request);
    }
}
