using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using System.Security.Claims;
using Auto_Insurance_Management_System.ViewModels; // Add this using statement

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPolicyService _policyService; // Inject IPolicyService

        public DashboardController(IAuthService authService, IPolicyService policyService) // Add IPolicyService to constructor
        {
            _authService = authService;
            _policyService = policyService; // Initialize IPolicyService
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _authService.GetUserProfileAsync(userId); // Renamed for clarity

            if (currentUser == null)
            {
                // Handle case where user profile isn't found (e.g., redirect to login)
                return RedirectToAction("Login", "Account"); // Adjust to your actual Login path
            }

            // Create the new DashboardViewModel
            var dashboardViewModel = new DashboardViewModel
            {
                CurrentUser = currentUser
            };

            // Fetch policies ONLY if the current user is an ADMIN or AGENT (if agents also need to see all policies)
            // Based on your Index.cshtml, only ADMIN can delete, but AGENT can also manage policies.
            // If you want ALL policies for both ADMIN and AGENT, keep this.
            // If only ADMIN should see ALL, adjust the condition.
            if (currentUser.Role == UserRole.ADMIN || currentUser.Role == UserRole.AGENT)
            {
                dashboardViewModel.AllPolicies = await _policyService.GetAllPoliciesAsync(null, null);
                // If you want search/filter to work on the dashboard, you would also need to pass
                // searchString and policyStatus parameters to this Index action and to GetAllPoliciesAsync,
                // and then populate ViewBag.PolicyStatuses from here. For minimum changes, we're just
                // displaying what GetAllPoliciesAsync returns initially. The search form will submit
                // to Policy/Index anyway, which is fine.
            }
            else if (currentUser.Role == UserRole.CUSTOMER)
            {
                // For customers, you'd typically show only their own policies
                dashboardViewModel.AllPolicies = await _policyService.GetPoliciesByUserIdAsync(userId, null, null);
            }

            return View(dashboardViewModel); // Pass the new combined ViewModel
        }

        // You can simplify these role-specific actions by just redirecting to Index,
        // as Index itself now handles fetching data based on the user's role.
        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public IActionResult AdminDashboard()
        {
            return RedirectToAction("Index"); // Redirect to the main Index action
        }

        [Authorize(Roles = nameof(UserRole.AGENT))]
        public IActionResult AgentDashboard()
        {
            return RedirectToAction("Index"); // Redirect to the main Index action
        }

        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public IActionResult CustomerDashboard()
        {
            return RedirectToAction("Index"); // Redirect to the main Index action
        }
    }
}