using Calmative.Server.API.DTOs;
using Calmative.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace Calmative.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var (succeeded, _, errorMessage) = await _authService.RegisterAsync(registerDto);
            if (!succeeded)
            {
                return BadRequest(new { Message = errorMessage });
            }
            return Ok(new { Message = errorMessage }); // "Registration successful. Please check your email..."
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var (succeeded, token, errorMessage) = await _authService.LoginAsync(loginDto);
            if (!succeeded)
            {
                return Unauthorized(new { Message = errorMessage });
            }
            return Ok(new { Token = token });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var success = await _authService.ConfirmEmailAsync(userId, token);
            // Redirect to the Blazor app's landing page with the result
            var redirectUrl = $"https://localhost:7294/auth/confirm-email-landing?isSuccess={success}";
            return Redirect(redirectUrl);
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordDto dto)
        {
            var (succeeded, message) = await _authService.RequestPasswordResetAsync(dto.Email);
            // Always return OK to prevent email enumeration
            return Ok(new { Message = message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var (succeeded, message) = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!succeeded)
            {
                return BadRequest(new { Message = message });
            }
            return Ok(new { Message = message });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            {
                return Unauthorized();
            }

            var (succeeded, message) = await _authService.ChangePasswordAsync(id, changePasswordDto);

            if (!succeeded)
            {
                return BadRequest(new { Message = message });
            }

            return Ok(new { Message = message });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken()
        {
            try
            {
                // Get token from Authorization header
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { valid = false, message = "No Bearer token provided" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                // Validate token manually using JWT service
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
                
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                
                return Ok(new { 
                    valid = true, 
                    userId = userIdClaim,
                    email = emailClaim,
                    message = "Token is valid",
                    tokenLength = token.Length
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { valid = false, message = ex.Message, details = ex.ToString() });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working", timestamp = DateTime.UtcNow });
        }

        [HttpPost("debug-headers")]
        public IActionResult DebugHeaders()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            
            return Ok(new 
            { 
                message = "Header debug info",
                hasAuthHeader = !string.IsNullOrEmpty(authHeader),
                authHeaderValue = authHeader,
                authHeaderLength = authHeader?.Length ?? 0,
                allHeaders = headers,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("debug-token")]
        public IActionResult DebugToken([FromBody] DebugTokenRequest request)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
                
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);
                
                return Ok(new 
                { 
                    valid = true, 
                    claims = principal.Claims.Select(c => new { type = c.Type, value = c.Value }),
                    expires = validatedToken.ValidTo,
                    issuer = validatedToken.Issuer
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { valid = false, error = ex.Message });
            }
        }

        [HttpPost("create-test-user")]
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                var testUser = new RegisterDto
                {
                    FirstName = "Test",
                    LastName = "User", 
                    Email = "test@test.com",
                    Password = "Test123!",
                    ConfirmPassword = "Test123!"
                };

                var (succeeded, _, errorMessage) = await _authService.RegisterAsync(testUser);
                
                if (succeeded)
                {
                    return Ok(new { message = "Test user created successfully", errorMessage });
                }
                else
                {
                    return BadRequest(new { message = errorMessage ?? "Test user already exists or creation failed" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class DebugTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }

        public class ForgotPasswordDto 
        {
            public string Email { get; set; } = string.Empty;
        }
    }
} 