using ASP_Chat.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP_Chat.Service.Impl
{
    public class JwtService : IJwtService
    {
        public string GenerateJwtToken(string userId)
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
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public long GetUserIdFromToken(string? headers)
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
    }
}
