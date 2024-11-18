using ASP_Chat.Entity;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using ASP_Chat.Exception;

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

        public int DeleteUser(long id)
        {
            _logger.LogDebug("Deleting user with id: {id}", id);
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                throw new CustomException($"User with id: {id} not found",
                    CustomException.ExceptionCodes.UserNotFound, 
                    CustomException.StatusCodes.NotFound);
            }    

            _context.Users.Remove(user);
            return _context.SaveChanges();
        }

        public User GetUserById(long id)
        {
            _logger.LogDebug("Getting user with id: {id}", id);
            User? user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                throw new CustomException($"User with id: {id} not found", 
                    CustomException.ExceptionCodes.UserNotFound, 
                    CustomException.StatusCodes.NotFound);
            }

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

        public User UpdateUser(long id, string? username, string? name, string? description)
        {
            _logger.LogDebug("Updating user with id: {id}", id);
            User? user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                throw new CustomException($"User with id: {id} not found", 
                    CustomException.ExceptionCodes.UserNotFound, 
                    CustomException.StatusCodes.NotFound);
            }

            if (!string.IsNullOrEmpty(username)) 
            {
                user.Username = username;
            } 

            if (!string.IsNullOrEmpty(name))
            {
                user.Name = name;
            }

            if (!string.IsNullOrEmpty(description))
            {
                user.Description = description;
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }
    }
}
