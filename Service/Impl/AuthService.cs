using ASP_Chat.Controllers.Request;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ASP_Chat.Service.Impl
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDBContext _context;
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

        private void ValidateUser(User user, AuthLoginRequest request)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw ServerExceptionFactory.InvalidCredentials();
            }
        }

        public string Login(AuthLoginRequest request)
        {
            _logger.LogDebug("Username: {Username}. Try to login.", request.Username);

            User? user = _userService.GetUserByUsername(request.Username);

            if (user == null)
            {
                throw ServerExceptionFactory.InvalidCredentials();
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw ServerExceptionFactory.InvalidCredentials();
            }

            return _jwtService.GenerateJwtToken(user.Id.ToString());
        }

        public string Register(AuthRegisterRequest request)
        {
            _logger.LogDebug("Username: {Username}. Try to register.", request.Username);

            User? user = _userService.GetUserByUsername(request.Username);

            if (user != null)
            {
                throw ServerExceptionFactory.UserAlreadyExists();
            }

            User newUser = new User { Username = request.Username, Name = request.Name };
            newUser.Password = _passwordHasher.HashPassword(newUser, request.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return "User registered successfully";
        }

        public string ChangePassword(long id, AuthChangePasswordRequest request)
        {
            _logger.LogDebug("UserId: {Id}. Try to change password.", id);

            User user = _userService.GetUserById(id);

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw ServerExceptionFactory.InvalidCredentials();
            }

            user.Password = _passwordHasher.HashPassword(user, request.NewPassword);

            _context.Users.Update(user);
            _context.SaveChanges();

            return "Password changed successfully";
        }
    }
}
