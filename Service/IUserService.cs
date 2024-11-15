using ASP_Chat.Entity;
using System.Collections.ObjectModel;

namespace ASP_Chat.Service
{
    public interface IUserService
    {
        User GetUserById(long id);
        Collection<User> GetUsersByUsername(string username);
        User UpdateUser(long id, string? username, string? password, string? name, string? description, string? image);
        int DeleteUser(long id);
    }
}
