using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Auto_Insurance_Management_System.Models; // Ensure User and UserRole, Policy (if needed for direct model use) are defined here
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Reflection.Metadata;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly IAuthService _authService; // Your existing AuthService for getting all users

        public PolicyController(IPolicyService policyService, IAuthService authService)
        {
            _policyService = policyService;
            _authService = authService;
        }

        // GET: Policy/Index
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

        // GET: Policy/Details/{id}
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

        // GET: Policy/Create
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Create()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                PolicyNumber = GeneratePolicyNumber() // Generate default or leave empty for user input
            };
            await PopulateCustomersForViewBag();

            // --- ADD DROPDOWN DATA HERE FOR CREATE ---
            // Vehicle Model dropdown is skipped for now as per your request.
            PopulateFixedDropdowns(null, null, null);
            // --- END ADD DROPDOWN DATA HERE FOR CREATE ---

            return View(model);
        }

        // POST: Policy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Create(CreatePolicyViewModel model)
        {
            await PopulateCustomersForViewBag();

            // --- RE-POPULATE DROPDOWN DATA ON POST BACK FOR CREATE ---
            // Vehicle Model dropdown is skipped for now as per your request.
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);
            // --- END RE-POPULATE DROPDOWN DATA ON POST BACK FOR CREATE ---

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

        // GET: Policy/Edit/{id}
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id)
        {
            var policyDetails = await _policyService.GetPolicyByIdAsync(id);
            if (policyDetails == null)
            {
                return NotFound();
            }

            // Attempt to parse VehicleDetails back into individual fields
            string[] vehicleParts = policyDetails.VehicleDetails.Split(' ');
            string vehicleMake = "";
            string vehicleModel = "";
            int vehicleYear = 0;
            string licensePlate = "";

            if (vehicleParts.Length >= 4 && policyDetails.VehicleDetails.Contains("(") && policyDetails.VehicleDetails.EndsWith(")"))
            {
                try
                {
                    // Assuming format "Year Make Model (LicensePlate)"
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
                    // Handle cases where parsing fails, e.g., default values or log error
                    Console.WriteLine("Warning: Could not parse VehicleDetails string into expected format.");
                }
            }
            else
            {
                Console.WriteLine("Warning: VehicleDetails string not in expected 'Year Make Model (LicensePlate)' format.");
                // You might want to log this or set default values for the edit form
            }


            var model = new CreatePolicyViewModel
            {
                PolicyId = policyDetails.PolicyId,
                UserId = policyDetails.UserId,
                PolicyNumber = policyDetails.PolicyNumber,
                PremiumAmount = policyDetails.PremiumAmount,
                StartDate = policyDetails.StartDate,
                EndDate = policyDetails.EndDate,
                Status = policyDetails.PolicyStatus, // Use actual status from policyDetails for Edit
                VehicleMake = vehicleMake,
                VehicleModel = vehicleModel,
                VehicleYear = vehicleYear,
                LicensePlate = licensePlate,
                CoverageType = policyDetails.CoverageType,
                CoverageAmount = policyDetails.CoverageAmount
            };

            await PopulateCustomersForViewBag(model.UserId);

            // --- ADD DROPDOWN DATA HERE FOR EDIT ---
            // Vehicle Model dropdown is skipped for now as per your request.
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);
            // --- END ADD DROPDOWN DATA HERE FOR EDIT ---

            return View(model);
        }

        // POST: Policy/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(UserRole.ADMIN)},{nameof(UserRole.AGENT)}")]
        public async Task<IActionResult> Edit(int id, CreatePolicyViewModel model)
        {
            await PopulateCustomersForViewBag(model.UserId);

            // --- RE-POPULATE DROPDOWN DATA ON POST BACK FOR EDIT ---
            // Vehicle Model dropdown is skipped for now as per your request.
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);
            // --- END RE-POPULATE DROPDOWN DATA ON POST BACK FOR EDIT ---

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

        /*### Policy Deletion Actions

        These actions handle the display of the delete confirmation page and the actual deletion.

        **GET: Policy/Delete/{ id}**
        This action displays the confirmation page for deleting a policy.
        It's authorized for administrators only.
        */
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.ADMIN))] // Only Admins can access this view
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve policy details using the service layer
            var policy = await _policyService.GetPolicyByIdAsync(id.Value); // .Value because id is nullable

            if (policy == null)
            {
                return NotFound();
            }

            // The PolicyDetailsViewModel is already suitable for display
            return View(policy);
        }

        /*
        **POST: Policy/Delete/{id}**
        This action handles the actual deletion of the policy from the database.
        It's authorized for administrators only and includes anti-forgery token validation.
        */
        [HttpPost, ActionName("Delete")] // ActionName is used so that the HTTP POST method can still be called "Delete" from the view
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.ADMIN))] // Only Admins can perform this action
        public async Task<IActionResult> DeleteConfirmed(int PolicyId) // Parameter name matches asp-for="PolicyId" in the form
        {
            // Use the service layer to delete the policy
            var result = await _policyService.DeletePolicyAsync(PolicyId);
            if (result)
            {
                TempData["Success"] = "Policy deleted successfully!"; // Success message
            }
            else
            {
                TempData["Error"] = "Failed to delete policy. Policy not found or an error occurred."; // Error message
            }
            return RedirectToAction(nameof(Index)); // Redirect to the policy list after deletion
        }


        // Helper method to populate customers for dropdown
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

        // --- UPDATED HELPER METHOD FOR POPULATING DROPDOWNS (Vehicle Model excluded) ---
        private void PopulateFixedDropdowns(string selectedMake = null, int? selectedYear = null, string selectedCoverageType = null)
        {
            // Vehicle Makes
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

            // Vehicle Years (from 2005 to current year + 1 for future models)
            var currentYear = DateTime.Now.Year;
            var years = new List<SelectListItem>();
            for (int i = currentYear + 1; i >= 2005; i--) // From current year + 1 back to 2005
            {
                years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString(), Selected = (selectedYear == i) });
            }
            ViewBag.VehicleYears = years;

            // Coverage Types
            ViewBag.CoverageTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "LIABILITY", Text = "Liability", Selected = (selectedCoverageType == "LIABILITY") },
                new SelectListItem { Value = "COLLISION", Text = "Collision", Selected = (selectedCoverageType == "COLLISION") },
                new SelectListItem { Value = "COMPREHENSIVE", Text = "Comprehensive", Selected = (selectedCoverageType == "COMPREHENSIVE") },
                new SelectListItem { Value = "UNINSURED_MOTORIST", Text = "Uninsured Motorist", Selected = (selectedCoverageType == "UNINSURED_MOTORIST") },
                new SelectListItem { Value = "PERSONAL_INJURY_PROTECTION", Text = "Personal Injury Protection", Selected = (selectedCoverageType == "PERSONAL_INJURY_PROTECTION") }
            };
        }
        // --- END UPDATED HELPER METHOD FOR POPULATING DROPDOWNS ---


        // GET: Policy/Request (for Customer)
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public IActionResult Request()
        {
            var model = new CreatePolicyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
                // Do NOT set PolicyNumber here if it's generated on submit, or if you want it empty for input
                // Do NOT set Status here, it's set on POST
            };
            PopulateFixedDropdowns(null, null, null);
            // No need to PopulateCustomersForViewBag here for customer's own request
            return View(model);
        }

        // POST: Policy/Request (for Customer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(UserRole.CUSTOMER))]
        public async Task<IActionResult> Request(CreatePolicyViewModel model)
        {
            // Re-populate dropdowns immediately for postback scenario
            PopulateFixedDropdowns(model.VehicleMake, model.VehicleYear, model.CoverageType);

            // --- ADDRESS THE NEW VALIDATION ERRORS HERE ---

            // 1. Policy Status: Remove its error as we set it programmatically
            if (ModelState.ContainsKey(nameof(model.Status)))
            {
                ModelState.Remove(nameof(model.Status));
            }
            model.Status = "PENDING"; // Ensure this is set before ModelState.IsValid

            // 2. Customer ID: Remove its error as it's assigned from the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // This shouldn't happen with [Authorize], but as a fallback
                ModelState.AddModelError("", "User not identified. Please log in again.");
                return View(model);
            }
            if (ModelState.ContainsKey(nameof(model.UserId))) // Assuming your ViewModel has a UserId property
            {
                ModelState.Remove(nameof(model.UserId));
            }
            model.UserId = userId; // Assign the User ID here

            // 3. Policy Number: Generate and assign it here, then remove its error
            if (ModelState.ContainsKey(nameof(model.PolicyNumber)))
            {
                ModelState.Remove(nameof(model.PolicyNumber));
            }
            model.PolicyNumber = GeneratePolicyNumber(); // Use your helper to generate it

            // --- END ADDRESSING NEW VALIDATION ERRORS ---


            if (ModelState.IsValid) // Now check ModelState.IsValid after correcting programmatically set fields
            {
                if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                    return View(model);
                }

                // userId is already set above
                // model.Status is already "PENDING"
                // model.PolicyNumber is already generated

                var result = await _policyService.CreatePolicyAsync(model, userId); // Pass userId if service needs it

                if (result)
                {
                    TempData["Success"] = "Policy request submitted!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to submit policy request.";
            }

            // If ModelState.IsValid is false here, it means other fields (like VehicleMake, LicensePlate etc.) are invalid.
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



        // Helper method to generate a policy number
        private string GeneratePolicyNumber()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }
}