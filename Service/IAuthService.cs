using ASP_Chat.Controllers.Request;
using ASP_Chat.Controllers.Response;

namespace ASP_Chat.Service
{
    public interface IAuthService
    {
        LoginResponse Login(AuthLoginRequest request);
        string Register(AuthRegisterRequest request);
        string ChangePassword(long id, AuthChangePasswordRequest request);
    }
}
