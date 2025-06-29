using Calmative.Server.API.DTOs;

namespace Calmative.Server.API.Services
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string? Token, string? ErrorMessage)> RegisterAsync(RegisterDto registerDto);
        Task<(bool Succeeded, string? Token, string? ErrorMessage)> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetUserProfileAsync(int userId);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<(bool Succeeded, string? ErrorMessage)> RequestPasswordResetAsync(string email);
        Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    }
} 