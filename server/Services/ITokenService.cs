using System.Security.Claims;

namespace Server.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
    string HashToken(string token);
}
