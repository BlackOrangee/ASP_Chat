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
            User? user = _context.GetUserById(id);
            
            ThrowIfUserNotExists(user);

            _context.RemoveAndSave(user);
            return "User deleted successfully";
        }

        public User GetUserById(long id)
        {
            _logger.LogDebug("Getting user with id: {Id}", id);
            User? user = _context.GetUserById(id);

            ThrowIfUserNotExists(user);

            return user;
        }

        public User? GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user with username: {Username}", username);
            return _context.GetUserByUsername(username);
        }

        public HashSet<User> GetUsersByUsername(string username)
        {
            _logger.LogDebug("Getting users with same username: {Username}", username);
            return _context.GetUsersByUsername(username).ToHashSet();
        }

        public User UpdateUser(UserRequest request)
        {
            request.Validate();
            _logger.LogDebug("Updating user with id: {Id}", request.UserId);
            User? user = _context.GetUserById(request.UserId);

            ThrowIfUserNotExists(user);

            user.UpdateUsernameIfExists(request);

            user.UpdateNameIfExists(request);

            user.UpdateDescriptionIfExists(request);

            _context.UpdateAndSave(user);

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
