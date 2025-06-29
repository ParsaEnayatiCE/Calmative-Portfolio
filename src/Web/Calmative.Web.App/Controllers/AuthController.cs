using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calmative.Web.App.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Check if user is already logged in
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _apiService.Login(model);

            if (success)
            {
                // The token is stored in ApiService internally
                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", errorMessage ?? "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _apiService.Register(model);

            if (success)
            {
                TempData["SuccessMessage"] = message ?? "Registration successful! Please check your email to confirm your account.";
                return RedirectToAction("RegisterConfirmation");
            }
            else
            {
                ModelState.AddModelError("", message ?? "Registration failed.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Please enter your email address.");
                return View();
            }

            var (success, message) = await _apiService.RequestPasswordReset(email);

            if (success)
            {
                TempData["SuccessMessage"] = message ?? "Password reset link has been sent to your email.";
                return View("ForgotPasswordConfirmation");
            }
            else
            {
                ModelState.AddModelError("", message ?? "Failed to send password reset link.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _apiService.ResetPassword(model);

            if (success)
            {
                TempData["SuccessMessage"] = message ?? "Your password has been reset successfully!";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", message ?? "Failed to reset password.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Clear any cookies if needed
            Response.Cookies.Delete("jwt_token");
            
            TempData["SuccessMessage"] = "You have been logged out successfully!";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _apiService.GetUserProfile();

            if (user != null)
            {
                return View(user);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to load user profile.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            var (success, message) = await _apiService.ChangePassword(model);
            return Json(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var (success, message) = await _apiService.ConfirmEmail(userId, token);
            ViewData["Success"] = success;
            ViewData["Message"] = message;
            return View();
        }

        private bool IsUserLoggedIn()
        {
            return Request.Cookies.ContainsKey("jwt_token") && 
                   !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
        }
    }
} 