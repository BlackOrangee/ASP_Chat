using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;

namespace ASP_Chat.Service
{
    public interface IUserService
    {
        User GetUserById(long id);
        User? GetUserByUsername(string username);
        HashSet<User> GetUsersByUsername(string username);
        User UpdateUser(long userId, UserUpdateRequest request);
        string DeleteUser(long id);
    }
}
