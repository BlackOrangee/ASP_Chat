using ASP_Chat.Entity;
using ASP_Chat.Exception;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP_Chat.Service.Impl
{
    public class AuthService : IAuthService
    {
        private ApplicationDBContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserService _userService;

        public AuthService(ApplicationDBContext context, ILogger<AuthService> logger, IPasswordHasher<User> passwordHasher,
            IUserService userService)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _userService = userService;
        }

        private string GenerateJwtToken(string userId)
        {
            DotNetEnv.Env.Load();

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new CustomException("JWT secret key is not set",
                    CustomException.ExceptionCodes.SecretKeyNotSet,
                    CustomException.StatusCodes.InternalServerError);
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                //issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
                //audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string Login(string username, string password)
        {
            _logger.LogDebug($"Username: {username}. Try to login.");

            User? user = _userService.GetUserByUsername(username);

            if (user == null)
            {
                throw new CustomException($"Wrong username or password",
                    CustomException.ExceptionCodes.InvalidCredentials, 
                    CustomException.StatusCodes.BadRequest);
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
            { 
                throw new CustomException("Wrong username or password", 
                    CustomException.ExceptionCodes.InvalidCredentials, 
                    CustomException.StatusCodes.BadRequest);
            }

            return GenerateJwtToken(user.Id.ToString());
        }

        public string Register(string username, string password, string name)
        {
            _logger.LogDebug($"Username: {username}. Try to register.");

            User? user = _userService.GetUserByUsername(username);

            if (user != null)
            {
                throw new CustomException($"User with this username {username} already exists", 
                    CustomException.ExceptionCodes.UserAlreadyExists, 
                    CustomException.StatusCodes.BadRequest);
            }

            User newUser = new User { Username = username, Name = name };
            newUser.Password = _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return "User registered successfully";
        }

        public string ChangePassword(long userId, string oldPassword, string newPassword)
        {
            _logger.LogDebug($"UserId: {userId}. Try to change password.");

            User user = _userService.GetUserById(userId);

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Failed)
            {
                throw new CustomException("Invalid password",
                    CustomException.ExceptionCodes.InvalidCredentials,
                    CustomException.StatusCodes.BadRequest);
            }

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            return "Password changed successfully";
        }
    }
}
