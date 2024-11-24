using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
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

        public static long GetUserIdFromToken(string? headers)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var token = headers.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(token);
                if (jsonToken is JwtSecurityToken tokenS)
                {
                    var userIdClaim = tokenS.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
                    if (userIdClaim == null)
                    {
                        throw new ServerException("Invalid token",
                            ServerException.ExceptionCodes.InvalidToken,
                            ServerException.StatusCodes.Unauthorized);
                    }

                    return long.Parse(userIdClaim.Value);
                }

                throw new ServerException("Invalid token format",
                    ServerException.ExceptionCodes.InvalidToken,
                    ServerException.StatusCodes.Unauthorized);
            }
            catch (Exception ex)
            {
                throw new ServerException("Failed to parse token",
                    ServerException.ExceptionCodes.InvalidToken,
                    ServerException.StatusCodes.Unauthorized);
            }
        }


        private string GenerateJwtToken(string userId)
        {
            DotNetEnv.Env.Load();

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ServerException("JWT secret key is not set",
                    ServerException.ExceptionCodes.SecretKeyNotSet,
                    ServerException.StatusCodes.InternalServerError);
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
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ServerException("Username or password is empty", 
                    ServerException.ExceptionCodes.EmptyCredentials, 
                    ServerException.StatusCodes.BadRequest);
            }

            _logger.LogDebug($"Username: {username}. Try to login.");

            User? user = _userService.GetUserByUsername(username);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, password) 
                == PasswordVerificationResult.Failed)
            { 
                throw new ServerException("Wrong username or password", 
                    ServerException.ExceptionCodes.InvalidCredentials, 
                    ServerException.StatusCodes.BadRequest);
            }

            return GenerateJwtToken(user.Id.ToString());
        }

        public string Register(string username, string password, string name)
        {
            _logger.LogDebug($"Username: {username}. Try to register.");

            if (string.IsNullOrEmpty(username) 
                || string.IsNullOrEmpty(password) 
                || string.IsNullOrEmpty(name))
            {
                throw new ServerException("Username, password or name is empty", 
                    ServerException.ExceptionCodes.EmptyCredentials, 
                    ServerException.StatusCodes.BadRequest);
            }

            User? user = _userService.GetUserByUsername(username);

            if (user != null)
            {
                throw new ServerException($"User with this username {username} already exists", 
                    ServerException.ExceptionCodes.UserAlreadyExists, 
                    ServerException.StatusCodes.BadRequest);
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

            if (string.IsNullOrEmpty(oldPassword) 
                || string.IsNullOrEmpty(newPassword))
            {
                throw new ServerException("Password is empty", 
                    ServerException.ExceptionCodes.EmptyCredentials, 
                    ServerException.StatusCodes.BadRequest);
            }

            User user = _userService.GetUserById(userId);

            if (_passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Failed)
            {
                throw new ServerException("Invalid password",
                    ServerException.ExceptionCodes.InvalidCredentials,
                    ServerException.StatusCodes.BadRequest);
            }

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            return "Password changed successfully";
        }
    }
}
