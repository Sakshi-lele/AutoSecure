using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using System.Security.Claims;

namespace Auto_Insurance_Management_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.LoginAsync(model);
                if (result)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterUserAsync(model);
                if (result)
                {
                    TempData["Success"] = "Registration successful. Please login.";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError(string.Empty, "Registration failed.");
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _authService.GetUserProfileAsync(userId);
            return View(user);
        }

        // --- NEW ADDITION FOR ACCESS DENIED ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // This action is called when an authenticated user tries to access a resource
            // for which they do not have the required authorization (e.g., role).
            // It will look for Views/Auth/AccessDenied.cshtml
            return View();
        }
        // --- END NEW ADDITION ---
    }
}