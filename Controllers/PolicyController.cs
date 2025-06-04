using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auto_Insurance_Management_System.Models;
using System.Linq;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;


        public PolicyController(IPolicyService policyService, UserManager<User> userManager, IAuthService authService)
        {
            _policyService = policyService;
            _userManager = userManager;
            _authService = authService;
        }

        // GET: Policy
        public async Task<IActionResult> Index(string search = "", string status = "")
        {
               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.GetUserProfileAsync(userId);
            
            List<PolicyDetailsViewModel> policies;

            if (User.IsInRole(nameof(UserRole.ADMIN)))
            {
                policies = await _policyService.GetAllPoliciesAsync(search, status);
            }
            else if (User.IsInRole(nameof(UserRole.AGENT)))
            {
                policies = await _policyService.GetAllPoliciesAsync(search, status);
            }
            else // Customer role
            {
                policies = await _policyService.GetPoliciesByUserIdAsync(userId, search, status);
            }

            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status;
            ViewData["Title"] = "Policies";
            ViewBag.User = user;

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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Forbid();
            }

            if (!User.IsInRole(nameof(UserRole.ADMIN)) && !User.IsInRole(nameof(UserRole.AGENT)) && policy.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(policy);
        }

        // GET: Policy/Create
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)},{nameof(UserRole.CUSTOMER)}")]
        public IActionResult Create()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            return View(model);
        }

        // POST: Policy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)},{nameof(UserRole.CUSTOMER)}")]
        public async Task<IActionResult> Create(CreatePolicyViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _policyService.CreatePolicyAsync(model, userId);

                if (result)
                {
                    TempData["Success"] = "Policy created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = "Failed to create policy. Please try again.";
            }

            return View(model);
        }

        // GET: Policy/Edit/5
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

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

            return View(model);
        }

        // POST: Policy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id, CreatePolicyViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                var result = await _policyService.UpdatePolicyAsync(id, model);
                if (result)
                {
                    TempData["Success"] = "Policy updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                
                TempData["Error"] = "Failed to update policy. Please try again.";
            }

            return View(model);
        }

        // POST: Policy/UpdateStatus
        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var processedBy = User.Identity.Name;
            if (string.IsNullOrEmpty(processedBy))
            {
                processedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            return View(policy);
        }

        // POST: Policy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
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
                return Forbid();
            }

            var model = new CreateClaimViewModel
            {
                PolicyId = policyId,
                IncidentDate = DateTime.Today
            };

            ViewBag.PolicyNumber = policy.PolicyNumber;

            return View(model);
        }

        // POST: Policy/CreateClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public async Task<IActionResult> CreateClaim(CreateClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IncidentDate > DateTime.Today)
                {
                    ModelState.AddModelError("IncidentDate", "Incident date cannot be in the future.");
                    var policy = await _policyService.GetPolicyByIdAsync(model.PolicyId);
                    ViewBag.PolicyNumber = policy?.PolicyNumber;
                    return View(model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _policyService.CreateClaimAsync(model, userId);

                if (result)
                {
                    TempData["Success"] = "Claim submitted successfully!";
                    return RedirectToAction("MyClaims");
                }
                
                TempData["Error"] = "Failed to submit claim. Please try again.";
            }

            var failedPolicy = await _policyService.GetPolicyByIdAsync(model.PolicyId);
            ViewBag.PolicyNumber = failedPolicy?.PolicyNumber;
            return View(model);
        }

        // GET: Policy/MyClaims
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public async Task<IActionResult> MyClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _policyService.GetClaimsByUserIdAsync(userId);
            return View(claims);
        }

        // GET: Policy/AllClaims
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> AllClaims()
        {
            var claims = await _policyService.GetAllClaimsAsync();
            return View(claims);
        }

        // POST: Policy/ProcessClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> ProcessClaim(int claimId, string status, string? rejectionReason = null)
        {
            var processedBy = User.Identity.Name;
            if (string.IsNullOrEmpty(processedBy))
            {
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

            return RedirectToAction("AllClaims");
        }
    }
}