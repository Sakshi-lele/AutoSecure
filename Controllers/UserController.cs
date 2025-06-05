using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        // GET: User/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var result = await _authService.UpdateUserProfileAsync(model);
                if (result)
                {
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Failed to update user.";
            }
            return View(model);
        }

        // POST: User/Deactivate/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _authService.DeactivateUserAsync(id);
            if (result)
                TempData["Success"] = "User deactivated.";
            else
                TempData["Error"] = "Failed to deactivate user.";

            return RedirectToAction(nameof(Index));
        }
    }
}
