using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Auto_Insurance_Management_System.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly IAuthService _authService;

        public PolicyController(IPolicyService policyService, IAuthService authService)
        {
            _policyService = policyService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(string searchString, string policyStatus)
        {
            List<PolicyDetailsViewModel> policies;

            bool isAdminOrAgent = User.IsInRole(nameof(UserRole.ADMIN)) || User.IsInRole(nameof(UserRole.AGENT));

            if (isAdminOrAgent)
            {
                policies = await _policyService.GetAllPoliciesAsync(searchString, policyStatus);
            }
            else if (User.IsInRole(nameof(UserRole.CUSTOMER)))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                policies = await _policyService.GetPoliciesByUserIdAsync(userId, searchString, policyStatus);
            }
            else
            {
                return Forbid();
            }

            ViewBag.PolicyStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Statuses" },
                new SelectListItem { Value = "ACTIVE", Text = "Active" },
                new SelectListItem { Value = "EXPIRED", Text = "Expired" },
                new SelectListItem { Value = "CANCELLED", Text = "Cancelled" },
                new SelectListItem { Value = "PENDING", Text = "Pending" }
            };
            ViewBag.CurrentStatus = policyStatus;
            ViewBag.CurrentSearch = searchString;

            return View(policies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole(nameof(UserRole.ADMIN)) && !User.IsInRole(nameof(UserRole.AGENT)) && policy.UserId != userId)
            {
                return Forbid();
            }

            return View(policy);
        }

        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Create()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                PolicyNumber = GeneratePolicyNumber()
            };
            await PopulateCustomersForViewBag();
            PopulateFixedDropdowns(null, null, null);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Create(CreatePolicyViewModel model)
        {
            await PopulateCustomersForViewBag();
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);

            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                var result = await _policyService.CreatePolicyAsync(model, model.UserId);
                if (result)
                {
                    TempData["Success"] = "Policy created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Failed to create policy. Please try again.";
            }
            return View(model);
        }

        [Authorize(Roles = $"{nameof(UserRole.CUSTOMER)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id)
        {
            var policyDetails = await _policyService.GetPolicyByIdAsync(id);
            if (policyDetails == null)
            {
                return NotFound();
            }

            // Parse VehicleDetails
            string[] vehicleParts = policyDetails.VehicleDetails.Split(' ');
            string vehicleMake = "";
            string vehicleModel = "";
            int vehicleYear = 0;
            string licensePlate = "";

            if (vehicleParts.Length >= 4 && policyDetails.VehicleDetails.Contains("(") && policyDetails.VehicleDetails.EndsWith(")"))
            {
                try
                {
                    vehicleYear = int.Parse(vehicleParts[0]);
                    vehicleMake = vehicleParts[1];
                    vehicleModel = vehicleParts[2];
                    licensePlate = policyDetails.VehicleDetails.Substring(
                        policyDetails.VehicleDetails.IndexOf('(') + 1,
                        policyDetails.VehicleDetails.Length - policyDetails.VehicleDetails.IndexOf('(') - 2
                    );
                }
                catch (FormatException)
                {
                    Console.WriteLine("Warning: Could not parse VehicleDetails string.");
                }
            }
            else
            {
                Console.WriteLine("Warning: VehicleDetails string format unexpected.");
            }

            var model = new CreatePolicyViewModel
            {
                PolicyId = policyDetails.PolicyId,
                UserId = policyDetails.UserId,
                PolicyNumber = policyDetails.PolicyNumber,
                PremiumAmount = policyDetails.PremiumAmount,
                StartDate = policyDetails.StartDate,
                EndDate = policyDetails.EndDate,
                Status = policyDetails.PolicyStatus,
                VehicleMake = vehicleMake,
                VehicleModel = vehicleModel,
                VehicleYear = vehicleYear,
                LicensePlate = licensePlate,
                CoverageType = policyDetails.CoverageType,
                CoverageAmount = policyDetails.CoverageAmount
            };

            await PopulateCustomersForViewBag(model.UserId);
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.CUSTOMER)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id, CreatePolicyViewModel model)
        {
            await PopulateCustomersForViewBag(model.UserId);
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);

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
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = "Failed to update policy. Please try again.";
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.AGENT))]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            if (policy == null) return NotFound();
            
            return View(policy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.AGENT))]
        public async Task<IActionResult> DeleteConfirmed(int PolicyId, string deleteReason, string otherReason = null)
        {
            if (string.IsNullOrWhiteSpace(deleteReason))
            {
                TempData["Error"] = "Delete reason is required.";
                return RedirectToAction("Delete", new { id = PolicyId });
            }
            
            // Combine reason if "Other" is selected
            if (deleteReason == "Other" && !string.IsNullOrWhiteSpace(otherReason))
            {
                deleteReason = otherReason;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _policyService.SoftDeletePolicyAsync(PolicyId, deleteReason, userId);
            
            if (result)
                TempData["Success"] = "Policy deleted successfully!";
            else
                TempData["Error"] = "Failed to delete policy.";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.AGENT))]
        public async Task<IActionResult> DeletedPolicies()
        {
            var deletedPolicies = await _policyService.GetDeletedPoliciesAsync();
            return View(deletedPolicies);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.AGENT))]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _policyService.RestorePolicyAsync(id);
            if (result)
                TempData["Success"] = "Policy restored successfully!";
            else
                TempData["Error"] = "Failed to restore policy.";
            
            return RedirectToAction(nameof(DeletedPolicies));
        }

        private async Task PopulateCustomersForViewBag(string selectedUserId = null)
        {
            var allUsers = await _authService.GetAllUsersAsync();
            var customers = allUsers.Where(u => u.Role == UserRole.CUSTOMER).ToList();

            ViewBag.Customers = customers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FirstName ?? ""} {u.LastName ?? ""} ({u.Email})",
                Selected = u.Id == selectedUserId
            }).OrderBy(item => item.Text).ToList();
        }

        private void PopulateFixedDropdowns(string selectedMake = null, int? selectedYear = null, string selectedCoverageType = null)
        {
            ViewBag.VehicleMakes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Toyota", Text = "Toyota", Selected = (selectedMake == "Toyota") },
                new SelectListItem { Value = "Honda", Text = "Honda", Selected = (selectedMake == "Honda") },
                new SelectListItem { Value = "Ford", Text = "Ford", Selected = (selectedMake == "Ford") },
                new SelectListItem { Value = "Chevrolet", Text = "Chevrolet", Selected = (selectedMake == "Chevrolet") },
                new SelectListItem { Value = "BMW", Text = "BMW", Selected = (selectedMake == "BMW") },
                new SelectListItem { Value = "Mercedes-Benz", Text = "Mercedes-Benz", Selected = (selectedMake == "Mercedes-Benz") },
                new SelectListItem { Value = "Hyundai", Text = "Hyundai", Selected = (selectedMake == "Hyundai") },
                new SelectListItem { Value = "Kia", Text = "Kia", Selected = (selectedMake == "Kia") },
                new SelectListItem { Value = "Nissan", Text = "Nissan", Selected = (selectedMake == "Nissan") },
                new SelectListItem { Value = "Volkswagen", Text = "Volkswagen", Selected = (selectedMake == "Volkswagen") },
                new SelectListItem { Value = "Audi", Text = "Audi", Selected = (selectedMake == "Audi") }
            };

            var currentYear = DateTime.Now.Year;
            var years = new List<SelectListItem>();
            for (int i = currentYear + 1; i >= 2005; i--)
            {
                years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = (selectedYear == i) });
            }
            ViewBag.VehicleYears = years;

            ViewBag.CoverageTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "LIABILITY", Text = "Liability", Selected = (selectedCoverageType == "LIABILITY") },
                new SelectListItem { Value = "COLLISION", Text = "Collision", Selected = (selectedCoverageType == "COLLISION") },
                new SelectListItem { Value = "COMPREHENSIVE", Text = "Comprehensive", Selected = (selectedCoverageType == "COMPREHENSIVE") },
                new SelectListItem { Value = "UNINSURED_MOTORIST", Text = "Uninsured Motorist", Selected = (selectedCoverageType == "UNINSURED_MOTORIST") },
                new SelectListItem { Value = "PERSONAL_INJURY_PROTECTION", Text = "Personal Injury Protection", Selected = (selectedCoverageType == "PERSONAL_INJURY_PROTECTION") }
            };
        }

        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public IActionResult Request()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };
            PopulateFixedDropdowns(null, null, null);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public async Task<IActionResult> Request(CreatePolicyViewModel model)
        {
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);

            if (ModelState.ContainsKey(nameof(model.Status)))
                ModelState.Remove(nameof(model.Status));
            model.Status = "PENDING";

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User not identified. Please log in again.");
                return View(model);
            }
            if (ModelState.ContainsKey(nameof(model.UserId)))
                ModelState.Remove(nameof(model.UserId));
            model.UserId = userId;

            if (ModelState.ContainsKey(nameof(model.PolicyNumber)))
                ModelState.Remove(nameof(model.PolicyNumber));
            model.PolicyNumber = GeneratePolicyNumber();

            if (ModelState.IsValid)
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                var result = await _policyService.CreatePolicyAsync(model, userId);
                if (result)
                {
                    TempData["Success"] = "Policy request submitted!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to submit policy request.";
            }
            return View(model);
        }

        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> Requests()
        {
            var requests = await _policyService.GetAllPoliciesAsync(null, "PENDING");
            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> Accept(int id)
        {
            await _policyService.UpdatePolicyStatusAsync(id, "ACTIVE", User.Identity.Name);
            TempData["Success"] = "Policy request accepted.";
            return RedirectToAction("Requests");
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.ADMIN))]
        public async Task<IActionResult> Decline(int id)
        {
            await _policyService.UpdatePolicyStatusAsync(id, "DECLINED", User.Identity.Name);
            TempData["Error"] = "Policy request declined.";
            return RedirectToAction("Requests");
        }

        private string GeneratePolicyNumber()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }
}