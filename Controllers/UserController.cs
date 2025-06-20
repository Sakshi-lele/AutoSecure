using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auto_Insurance_Management_System.Data; // Add this namespace
using Microsoft.EntityFrameworkCore; // Add this namespace
using Auto_Insurance_Management_System.ViewModels; 
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context; // Add this field

        // Update constructor to inject ApplicationDbContext
        public UserController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context; // Initialize context
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            // Clear TempData to prevent duplicate messages
            TempData.Clear();
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        // GET: User/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };

            return View(viewModel);
        }

        // POST: User/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Role = model.Role;

            var result = await _authService.UpdateUserProfileAsync(user);
            if (result)
            {
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update user. Please try again.";
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

         public async Task<IActionResult> Details(string id)
        {
            var user = await _authService.GetUserProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var duePayments = await GetDuePaymentsForUser(id);

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                DuePayments = duePayments
            };

            return View(viewModel);
        }

        private async Task<List<PaymentDetailsViewModel>> GetDuePaymentsForUser(string userId)
        {
            // Get active policies for the user
            var policies = await _context.Policies
                .Where(p => p.UserId == userId && p.PolicyStatus == "ACTIVE")
                .ToListAsync();

            var duePayments = new List<PaymentDetailsViewModel>();
            
            foreach (var policy in policies)
            {
                // Check if payment is due
                var lastPayment = await _context.Payments
                    .Where(p => p.PolicyId == policy.Id && p.Status == PaymentStatus.Completed)
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync();
                
                // If no payment in the last 30 days, it's due
                if (lastPayment == null || lastPayment.PaymentDate < DateTime.UtcNow.AddDays(-30))
                {
                    duePayments.Add(new PaymentDetailsViewModel
                    {
                        PolicyId = policy.Id,
                        PolicyNumber = policy.PolicyNumber,
                        PaymentAmount = policy.PremiumAmount,
                        DueDate = lastPayment == null ? 
                                  policy.StartDate.AddDays(30) : 
                                  lastPayment.PaymentDate.AddDays(30)
                    });
                }
            }
            
            return duePayments;
        }
    }
}