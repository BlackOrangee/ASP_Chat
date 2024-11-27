namespace ASP_Chat.Service
{
    public interface IJwtService
    {
        string GenerateJwtToken(string userId);
        long GetUserIdFromToken(string? headers);
    }
}
