using Calmative.Server.API.Models;

namespace Calmative.Server.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
} 