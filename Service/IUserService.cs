using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IUserService
    {
        User GetUserById(long id);
        User? GetUserByUsername(string username);
        HashSet<User> GetUsersByUsername(string username);
        User UpdateUser(long id, string? username, string? name, string? description);
        int DeleteUser(long id);
    }
}
