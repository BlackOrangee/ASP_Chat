using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ASP_Chat.Service.Impl
{
    public class AuthService : IAuthService
    {
        private ApplicationDBContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthService(ApplicationDBContext context, ILogger<AuthService> logger, IPasswordHasher<User> passwordHasher,
            IUserService userService, IJwtService jwtService)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _userService = userService;
            _jwtService = jwtService;
        }

        public string Login(AuthRequest userRequest)
        {
            _logger.LogDebug($"Username: {userRequest.Username}. Try to login.");

            User? user = _userService.GetUserByUsername(userRequest.Username);

            if ( user == null 
                || _passwordHasher.VerifyHashedPassword(user, user.Password, userRequest.Password) 
                == PasswordVerificationResult.Failed)
            { 
                throw ServerExceptionFactory.InvalidCredentials();
            }

            return _jwtService.GenerateJwtToken(user.Id.ToString());
        }

        public string Register(AuthRequest userRequest)
        {
            _logger.LogDebug($"Username: {userRequest.Username}. Try to register.");

            User? user = _userService.GetUserByUsername(userRequest.Username);

            if (user != null)
            {
                throw ServerExceptionFactory.UserAlreadyExists();
            }

            User newUser = new User { Username = userRequest.Username, Name = userRequest.Name };
            newUser.Password = _passwordHasher.HashPassword(newUser, userRequest.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return "User registered successfully";
        }

        public string ChangePassword(AuthRequest userRequest)
        {
            _logger.LogDebug($"UserId: {userRequest.Id}. Try to change password.");

            User user = _userService.GetUserById(userRequest.Id);

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, userRequest.Password)
                == PasswordVerificationResult.Failed)
            {
                throw ServerExceptionFactory.InvalidCredentials();
            }

            user.Password = _passwordHasher.HashPassword(user, userRequest.NewPassword);

            _context.Users.Update(user);
            _context.SaveChanges();

            return "Password changed successfully";
        }
    }
}
