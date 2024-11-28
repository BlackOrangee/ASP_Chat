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
            _logger.LogDebug("Deleting user with id: {id}", id);
            User? user = _context.Users.First(u => u.Id == id);
            
            CheckIfUserExists(user);

            _context.Users.Remove(user);
            _context.SaveChanges();
            return "User deleted successfully";
        }

        public User GetUserById(long id)
        {
            _logger.LogDebug("Getting user with id: {id}", id);
            User? user = _context.Users.First(u => u.Id == id);

            CheckIfUserExists(user);

            return user;
        }

        public User? GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user with username: {username}", username);
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public HashSet<User> GetUsersByUsername(string username)
        {
            _logger.LogDebug("Getting users with same username: {username}", username);
            return _context.Users.Where(u => u.Username.Contains(username)).ToHashSet();
        }

        public User UpdateUser(UserRequest request)
        {
            request.Validate();
            _logger.LogDebug("Updating user with id: {id}", request.UserId);
            User? user = _context.Users.First(u => u.Id == request.UserId);

            CheckIfUserExists(user);

            if (!string.IsNullOrEmpty(request.Username)) 
            {
                user.Username = request.Username;
            } 

            if (!string.IsNullOrEmpty(request.Name))
            {
                user.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                user.Description = request.Description;
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        private void CheckIfUserExists(User? user)
        {
            _logger.LogDebug("Checking if user exists with username: {username}", user.Username);

            if (user == null)
            {
                throw ServerExceptionFactory.UserNotFound();
            }
        }
    }
}
