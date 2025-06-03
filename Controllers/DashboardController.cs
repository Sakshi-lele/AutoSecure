using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using System.Security.Claims;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAuthService _authService;

        public DashboardController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.GetUserProfileAsync(userId);
            
            return View(user);
        }

        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> AdminDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.GetUserProfileAsync(userId);
            
            return View("Index", user);
        }

        [Authorize(Roles = nameof(UserRole.AGENT))]
        public async Task<IActionResult> AgentDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.GetUserProfileAsync(userId);
            
            return View("Index", user);
        }

        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public async Task<IActionResult> CustomerDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.GetUserProfileAsync(userId);
            
            return View("Index", user);
        }
    }
}