using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auto_Insurance_Management_System.Models;
using System.Linq; // Added for .Any() in model state checks

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize] // Ensures only authenticated users can access any action in this controller
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly UserManager<User> _userManager;

        public PolicyController(IPolicyService policyService, UserManager<User> userManager)
        {
            _policyService = policyService;
            _userManager = userManager;
        }

        // Helper method to set ViewBag.UserRole consistently
        private void SetUserRoleViewBag()
        {
            string currentUserRole = "CUSTOMER"; // Default role
            if (User.IsInRole("ADMIN"))
            {
                currentUserRole = "ADMIN";
            }
            else if (User.IsInRole("AGENT"))
            {
                currentUserRole = "AGENT";
            }
            ViewBag.UserRole = currentUserRole;
        }

        // GET: Policy
        public async Task<IActionResult> Index(string search = "", string status = "")
        {
            List<PolicyDetailsViewModel> policies;

            // Determine user's role and fetch policies accordingly
            if (User.IsInRole("ADMIN"))
            {
                policies = await _policyService.GetAllPoliciesAsync(search, status);
            }
            else if (User.IsInRole("AGENT"))
            {
                // Agents might only see policies assigned to them, or all policies.
                // Your current GetAllPoliciesAsync suggests they see all.
                policies = await _policyService.GetAllPoliciesAsync(search, status);
            }
            else // Customer role (and any other non-Admin/Agent roles due to [Authorize])
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                policies = await _policyService.GetPoliciesByUserIdAsync(userId, search, status);
            }

            // Pass the current user's role to the view using the standardized ViewBag.UserRole
            SetUserRoleViewBag(); // <--- Using the helper method
            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status;
            ViewData["Title"] = "Policies"; // Set the title for the page

            return View(policies);
        }

        // GET: Policy/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            // Check if user can access this policy (Admin, Agent, or owner)
            var currentUser = await _userManager.GetUserAsync(User); // Get the full User object
            if (currentUser == null)
            {
                // This shouldn't happen with [Authorize], but good for robustness
                return Forbid();
            }

            // Explicitly check roles and policy ownership
            if (!User.IsInRole("ADMIN") && !User.IsInRole("AGENT") && policy.UserId != currentUser.Id)
            {
                // If not Admin/Agent AND not the owner, forbid access
                return Forbid();
            }

            SetUserRoleViewBag(); // <--- Using the helper method for consistency

            return View(policy);
        }

        // GET: Policy/Create
        [Authorize(Roles = "ADMIN,AGENT,CUSTOMER")]
        public IActionResult Create()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(model);
        }

        // POST: Policy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,AGENT,CUSTOMER")]
        public async Task<IActionResult> Create(CreatePolicyViewModel model)
        {
            // Ensure ViewBag.UserRole is set before any return to view
            SetUserRoleViewBag();

            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model); // Return to view with error
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _policyService.CreatePolicyAsync(model, userId);

                if (result)
                {
                    TempData["Success"] = "Policy created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to create policy. Please try again.";
                    // Even if service fails, still return the model to the view
                }
            }

            // If ModelState is not valid or service failed, return the view with the model
            return View(model);
        }

        // GET: Policy/Edit/5
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            // Ensure only Admin/Agent can edit policies. No owner-edit for policies?
            // If customers could edit their own policies, you'd need a similar check as Details action.
            // Current [Authorize(Roles = "ADMIN,AGENT")] covers this.

            var model = new CreatePolicyViewModel
            {
                PolicyId = policy.PolicyId,
                VehicleDetails = policy.VehicleDetails,
                CoverageAmount = policy.CoverageAmount,
                CoverageType = policy.CoverageType,
                PremiumAmount = policy.PremiumAmount,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                UserId = policy.UserId
            };

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(model);
        }

        // POST: Policy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> Edit(int id, CreatePolicyViewModel model)
        {
            // Ensure ViewBag.UserRole is set before any return to view
            SetUserRoleViewBag();

            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model); // Return to view with error
                }

                var result = await _policyService.UpdatePolicyAsync(id, model);
                if (result)
                {
                    TempData["Success"] = "Policy updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = id }); // Redirect to details after edit
                }
                else
                {
                    TempData["Error"] = "Failed to update policy. Please try again.";
                }
            }

            // If ModelState is not valid or service failed, return the view with the model
            return View(model);
        }

        // POST: Policy/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // You might want to add ValidateAntiForgeryToken here if this is a form submission
            // or ensure it's called via AJAX with appropriate anti-forgery tokens.
            // For simple redirects/API calls, it might be omitted if not directly from a form.
            // [ValidateAntiForgeryToken] is recommended for POSTs that change state.

            var processedBy = User.Identity.Name; // Get the user's username
            if (string.IsNullOrEmpty(processedBy))
            {
                // Fallback or error if username not found (e.g., from an API context)
                processedBy = User.FindFirstValue(ClaimTypes.NameIdentifier); // Use ID as fallback
            }

            var result = await _policyService.UpdatePolicyStatusAsync(id, status, processedBy);

            if (result)
            {
                TempData["Success"] = "Policy status updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update policy status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Policy/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(policy);
        }

        // POST: Policy/Delete/5
        [HttpPost, ActionName("Delete")] // ActionName specifies which action to call for this POST
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id) // Renamed for clarity with ActionName
        {
            var result = await _policyService.DeletePolicyAsync(id);
            if (result)
            {
                TempData["Success"] = "Policy deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete policy.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Policy/CreateClaim/5
        [Authorize(Roles = "CUSTOMER")]
        public async Task<IActionResult> CreateClaim(int policyId)
        {
            var policy = await _policyService.GetPolicyByIdAsync(policyId);
            if (policy == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (policy.UserId != userId)
            {
                return Forbid(); // Ensure only the policy owner can create a claim for it
            }

            var model = new CreateClaimViewModel
            {
                PolicyId = policyId,
                IncidentDate = DateTime.Today
            };

            ViewBag.PolicyNumber = policy.PolicyNumber;

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(model);
        }

        // POST: Policy/CreateClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CUSTOMER")]
        public async Task<IActionResult> CreateClaim(CreateClaimViewModel model)
        {
            // Ensure ViewBag.UserRole is set before any return to view
            SetUserRoleViewBag();

            if (ModelState.IsValid)
            {
                if (model.IncidentDate > DateTime.Today)
                {
                    ModelState.AddModelError("IncidentDate", "Incident date cannot be in the future.");
                    // Need to re-fetch PolicyNumber if returning to view due to error
                    var policy = await _policyService.GetPolicyByIdAsync(model.PolicyId);
                    ViewBag.PolicyNumber = policy?.PolicyNumber;
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _policyService.CreateClaimAsync(model, userId);

                if (result)
                {
                    TempData["Success"] = "Claim submitted successfully!";
                    return RedirectToAction("MyClaims"); // Redirect to MyClaims after submission
                }
                else
                {
                    TempData["Error"] = "Failed to submit claim. Please try again.";
                }
            }

            // Re-fetch PolicyNumber if returning to view due to error or service failure
            var failedPolicy = await _policyService.GetPolicyByIdAsync(model.PolicyId);
            ViewBag.PolicyNumber = failedPolicy?.PolicyNumber;
            return View(model);
        }

        // GET: Policy/MyClaims
        [Authorize(Roles = "CUSTOMER")]
        public async Task<IActionResult> MyClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _policyService.GetClaimsByUserIdAsync(userId);

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(claims);
        }

        // GET: Policy/AllClaims
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> AllClaims()
        {
            var claims = await _policyService.GetAllClaimsAsync();

            SetUserRoleViewBag(); // <--- Using the helper method

            return View(claims);
        }

        // POST: Policy/ProcessClaim
        [HttpPost]
        [ValidateAntiForgeryToken] // Add this for robust security
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> ProcessClaim(int claimId, string status, string? rejectionReason = null)
        {
            var processedBy = User.Identity.Name; // Agent/Admin who processed it
            if (string.IsNullOrEmpty(processedBy))
            {
                // Fallback to ID if name isn't reliably available
                processedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var result = await _policyService.ProcessClaimAsync(claimId, status, processedBy, rejectionReason);

            if (result)
            {
                TempData["Success"] = "Claim processed successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to process claim.";
            }

            return RedirectToAction("AllClaims"); // Redirect back to the list of all claims
        }
    }
}