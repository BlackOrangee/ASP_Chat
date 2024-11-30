using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;

namespace ASP_Chat.Service.Impl
{
    public class UserService : IUserService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDBContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string DeleteUser(long id)
        {
            _logger.LogDebug("Deleting user with id: {Id}", id);
            User? user = _context.Users.FirstOrDefault(u => u.Id == id);

            ThrowIfUserNotExists(user);

            _context.Users.Remove(user);
            _context.SaveChanges();
            return "User deleted successfully";
        }

        public User GetUserById(long id)
        {
            _logger.LogDebug("Getting user with id: {Id}", id);
            User? user = _context.Users.FirstOrDefault(u => u.Id == id);

            ThrowIfUserNotExists(user);

            return user;
        }

        public User? GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user with username: {Username}", username);
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public HashSet<User> GetUsersByUsername(string username)
        {
            _logger.LogDebug("Getting users with same username: {Username}", username);
            return _context.Users.Where(u => u.Username.Contains(username)).ToHashSet();
        }

        public User UpdateUser(UserRequest request)
        {
            request.Validate();
            _logger.LogDebug("Updating user with id: {Id}", request.UserId);
            User? user = _context.Users.FirstOrDefault(u => u.Id == request.UserId);

            ThrowIfUserNotExists(user);

            user.UpdateFieldsIfExists(request);

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        private void ThrowIfUserNotExists(User? user)
        {
            if (user == null)
            {
                throw ServerExceptionFactory.UserNotFound();
            }
        }
    }
}
