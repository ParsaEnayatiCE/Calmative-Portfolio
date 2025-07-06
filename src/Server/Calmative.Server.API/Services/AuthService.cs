using AutoMapper;
using BCrypt.Net;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Calmative.Server.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IMapper mapper, IJwtService jwtService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<(bool Succeeded, string? Token, string? ErrorMessage)> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return (false, null, "User with this email already exists.");
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            
            user.ConfirmationToken = GenerateUrlSafeToken();
            user.ConfirmationTokenExpiryTime = DateTime.UtcNow.AddHours(24);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send confirmation email
            var baseUrl = _configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5297";
            if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "http://" + baseUrl.Substring("https://".Length);
            }
            var confirmationLink = $"{baseUrl}/Auth/ConfirmEmail?userId={user.Id}&token={HttpUtility.UrlEncode(user.ConfirmationToken)}";
            var emailBody = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);
            
            return (true, null, "Registration successful. Please check your email to confirm your account.");
        }

        public async Task<(bool Succeeded, string? Token, string? ErrorMessage)> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, null, "Invalid email or password.");
            }

            if (!user.IsEmailConfirmed)
            {
                return (false, null, "Email not confirmed. Please check your inbox.");
            }

            var token = _jwtService.GenerateToken(user);
            return (true, token, null);
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            if (!int.TryParse(userId, out var id))
            {
                return false;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.ConfirmationToken != token || user.ConfirmationTokenExpiryTime <= DateTime.UtcNow)
            {
                return false;
            }

            user.IsEmailConfirmed = true;
            user.ConfirmationToken = null;
            user.ConfirmationTokenExpiryTime = null;

            // Create default portfolio if one doesn't exist
            var hasPortfolio = await _context.Portfolios.AnyAsync(p => p.UserId == user.Id);
            if (!hasPortfolio)
            {
                var defaultPortfolio = new Portfolio
                {
                    Name = "My First Portfolio",
                    Description = "Default portfolio created upon email confirmation.",
                    UserId = user.Id,
                };
                _context.Portfolios.Add(defaultPortfolio);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> RequestPasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return (true, "If an account with this email exists, a password reset link has been sent.");
            }

            user.PasswordResetToken = GenerateUrlSafeToken();
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var baseUrl = _configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5297";
            if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "http://" + baseUrl.Substring("https://".Length);
            }
            var resetLink = $"{baseUrl}/Auth/ResetPassword?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(user.PasswordResetToken)}";
            var emailBody = $"You can reset your password by <a href='{resetLink}'>clicking here</a>.";
            await _emailService.SendEmailAsync(user.Email, "Reset your password", emailBody);

            return (true, "If an account with this email exists, a password reset link has been sent.");
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.PasswordResetToken != dto.Token || user.PasswordResetTokenExpiryTime <= DateTime.UtcNow)
            {
                return (false, "Invalid token or email.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;
            await _context.SaveChangesAsync();

            return (true, "Your password has been reset successfully.");
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return (false, "Invalid current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Password changed successfully.");
        }

        public async Task<UserDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return _mapper.Map<UserDto>(user);
        }

        private static string GenerateUrlSafeToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
} 