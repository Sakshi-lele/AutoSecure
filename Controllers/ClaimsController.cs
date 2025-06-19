// Controllers/ClaimsController.cs
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Auto_Insurance_Management_System.Data;


namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = "CUSTOMER")]
    public class ClaimsController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IPolicyService _policyService;

        public ClaimsController(IClaimService claimService, IPolicyService policyService)
        {
            _claimService = claimService;
            _policyService = policyService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _claimService.GetClaimsByUserIdAsync(userId);
            
            return View(claims);
        }

        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var policies = await _policyService.GetPoliciesByUserIdAsync(userId, "", "");

            var model = new QuickClaimFormViewModel
            {
                UserId = userId,
                Policies = policies.Select(p => new SelectListItem
                {
                    Value = p.PolicyNumber,
                    Text = $"{p.PolicyNumber} - {p.VehicleDetails}"
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuickClaimFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate policies if needed
        var policies = await _policyService.GetPoliciesByUserIdAsync(model.UserId, "", "");
                model.Policies = policies.Select(p => new SelectListItem
                {
                    Value = p.PolicyNumber,
                    Text = $"{p.PolicyNumber} - {p.VehicleDetails}"
                });
                return View(model);
            }

            var claim = await _claimService.CreateClaimAsync(model, model.UserId);
            if (claim == null)
            {
                ModelState.AddModelError("", "Error creating claim. Please try again.");
                return View(model);
            }

              TempData["SuccessMessage"] = "Claim submitted successfully!";
            return RedirectToAction("Index"); // Changed from Details to Index
        }
        [HttpGet]
public async Task<IActionResult> GetPolicyDetails(string policyNumber)
{
    var policy = await _policyService.GetPolicyByPolicyNumberAsync(policyNumber);
    if (policy == null)
    {
        return NotFound();
    }

    return Json(new {
        vehicleNumber = policy.LicensePlate, // Use LicensePlate for vehicle number
        startDate = policy.StartDate.ToString("yyyy-MM-dd"),
        endDate = policy.EndDate.ToString("yyyy-MM-dd")
    });
}

        public async Task<IActionResult> Details(int id)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    // Use the new service method that includes related entities
    var claim = await _claimService.GetClaimByIdWithDetailsAsync(id);
    
    // Add comprehensive null checks
    if (claim == null || claim.Policy == null || claim.Policy.User == null)
    {
        return NotFound();
    }

    // Verify the current user owns this claim
    if (claim.Policy.UserId != userId)
    {
        return Forbid();
    }

    var model = new ClaimDetailsViewModel
    {
        ClaimId = claim.ClaimId,  // Changed from claim.ClaimId
        PolicyNumber = claim.Policy.PolicyNumber,
        VehicleModel = claim.Policy.VehicleModel,
        VehicleNumber = claim.Policy.LicensePlate,
        CoverageAmount = claim.Policy.CoverageAmount,
        PolicyStartDate = claim.Policy.StartDate,
        PolicyEndDate = claim.Policy.EndDate,
        ClaimType = claim.ClaimType,
        DateOfSubmission = claim.DateOfSubmission,
        IncidentDate = claim.IncidentDate,
        IncidentTime = claim.IncidentTime,
        LocationOfIncident = claim.IncidentLocation,
        IncidentDescription = claim.IncidentDescription,
        ClaimAmountRequested = claim.ClaimAmountRequested,
        DamageEstimate = claim.DamageEstimate,
        Status = claim.Status,
        UploadedDocuments = claim.Documents.ToList(),
        CustomerFullName = $"{claim.Policy.User.FirstName} {claim.Policy.User.LastName}",
        CustomerEmail = claim.Policy.User.Email,
        CustomerUserId = claim.Policy.UserId
    };

    return View(model);
}
    }
}