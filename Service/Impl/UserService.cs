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

            ThrowExceptionIfUserNotExists(user);

            _context.Users.Remove(user);
            _context.SaveChanges();
            return "User deleted successfully";
        }

        public User GetUserById(long id)
        {
            _logger.LogDebug("Getting user with id: {Id}", id);
            User? user = _context.Users.FirstOrDefault(u => u.Id == id);

            ThrowExceptionIfUserNotExists(user);

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

        public User UpdateUser(long userId, UserUpdateRequest request)
        {
            _logger.LogDebug("Updating user with id: {Id}", userId);
            User? user = _context.Users.FirstOrDefault(u => u.Id == userId);

            ThrowExceptionIfUserNotExists(user);

            ThrowExceptionIfUsernameTaken(request);

            user.UpdateFieldsIfExists(request);

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        public static void ThrowExceptionIfUserNotExists(User? user)
        {
            if (user == null)
            {
                throw ServerExceptionFactory.UserNotFound();
            }
        }

        private void ThrowExceptionIfUsernameTaken(UserUpdateRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Username)
                && _context.Users.FirstOrDefault(u => u.Username == request.Username) != null)
            {
                throw ServerExceptionFactory.UniqueNameIsTaken(request.Username);
            }
        }
    }
}
