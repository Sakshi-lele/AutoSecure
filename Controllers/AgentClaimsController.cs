// Controllers/AgentClaimsController.cs
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Auto_Insurance_Management_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = "AGENT")]
    public class AgentClaimsController : Controller
    {
        private readonly IClaimService _claimService;

        public AgentClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        public async Task<IActionResult> Index()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _claimService.GetClaimsForAgentAsync(agentId);
            return View(claims);
        }

        public async Task<IActionResult> Details(int id)
        {
            var claim = await _claimService.GetClaimByIdWithDetailsAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            var model = new ClaimDetailsViewModel
            {
                ClaimId = claim.ClaimId,
                PolicyNumber = claim.Policy?.PolicyNumber ?? "N/A",
                VehicleModel = claim.Policy?.VehicleModel ?? "N/A",
                VehicleNumber = claim.Policy?.LicensePlate ?? "N/A",
                CoverageAmount = claim.Policy?.CoverageAmount ?? 0,
                PolicyStartDate = claim.Policy?.StartDate ?? DateTime.MinValue,
                PolicyEndDate = claim.Policy?.EndDate ?? DateTime.MinValue,
                ClaimType = claim.ClaimType,
                DateOfSubmission = claim.DateOfSubmission,
                IncidentDate = claim.IncidentDate,
                IncidentTime = claim.IncidentTime,
                LocationOfIncident = claim.IncidentLocation,
                IncidentDescription = claim.IncidentDescription,
                ClaimAmountRequested = claim.ClaimAmountRequested,
                DamageEstimate = claim.DamageEstimate,
                Status = claim.Status,
                UploadedDocuments = claim.Documents?.ToList(),
                CustomerFullName = claim.Policy?.User != null ? 
                    $"{claim.Policy.User.FirstName} {claim.Policy.User.LastName}" : "N/A"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ClaimStatus status)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claim = await _claimService.UpdateClaimStatusAsync(id, status, agentId);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Failed to update claim status. Please try again.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Claim status updated to {status} successfully!";
            }

            return RedirectToAction("Details", new { id });
        }
        // Controllers/AgentClaimsController.cs
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Verify(int id)
{
    var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var claim = await _claimService.UpdateClaimStatusAsync(id, ClaimStatus.Verified, agentId);

    if (claim == null)
    {
        TempData["ErrorMessage"] = "Failed to verify claim. Please try again.";
    }
    else
    {
        TempData["SuccessMessage"] = "Claim verified successfully!";
    }

    return RedirectToAction("Details", new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Decline(int id)
{
    var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var claim = await _claimService.UpdateClaimStatusAsync(id, ClaimStatus.Declined, agentId);

    if (claim == null)
    {
        TempData["ErrorMessage"] = "Failed to decline claim. Please try again.";
    }
    else
    {
        TempData["SuccessMessage"] = "Claim declined successfully!";
    }

    return RedirectToAction("Details", new { id });
}
    }
}