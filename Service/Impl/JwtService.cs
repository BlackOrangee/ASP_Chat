using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ASP_Chat.Exceptions;
using Microsoft.IdentityModel.Tokens;

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
                throw ServerExceptionFactory.SecretKeyNotSet();
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
                        throw ServerExceptionFactory.InvalidToken();
                    }

                    return long.Parse(userIdClaim.Value);
                }

                throw ServerExceptionFactory.InvalidToken();
            }
            catch (Exception ex)
            {
                throw ServerExceptionFactory.InvalidToken();
            }
        }
    }
}
