// Controllers/AdminClaimsController.cs
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Auto_Insurance_Management_System.Data;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminClaimsController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly ApplicationDbContext _context;

        public AdminClaimsController(IClaimService claimService, ApplicationDbContext context)
        {
            _claimService = claimService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
                 var claims = await _context.Claims
        .Include(c => c.Policy)
            .ThenInclude(p => p.User)
        .Include(c => c.VerifiedByAgent)
        .Where(c => c.Status != ClaimStatus.Declined) // Exclude declined claims
        .OrderByDescending(c => c.DateOfSubmission)
        .ToListAsync();
    
    return View(claims);
        }

        // Changed to use "id" parameter name to match default routing
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id) // Changed parameter name to "id"
        {
            // Ensure you include related entities
            var claim = await _context.Claims
                .Include(c => c.Policy)
                    .ThenInclude(p => p.User)
                .Include(c => c.VerifiedByAgent) // Include agent details
                .Include(c => c.ApprovedByAdmin) // Include admin details
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == id); // Use "id" here

            if (claim == null)
            {
                return NotFound();
            }

            // Safely handle null references
            var model = new ClaimDetailsViewModel
            {
                ClaimId = claim.ClaimId,
                PolicyNumber = claim.Policy?.PolicyNumber ?? "N/A",
                VehicleModel = claim.Policy?.VehicleModel ?? "N/A",
                VehicleNumber = claim.Policy?.LicensePlate ?? "N/A",
                CoverageAmount = claim.Policy?.CoverageAmount ?? 0,
                PolicyStartDate = claim.Policy?.StartDate ?? DateTime.MinValue,
                PolicyEndDate = claim.Policy?.EndDate ?? DateTime.MinValue,
                Status = claim.Status,
                DateOfSubmission = claim.DateOfSubmission,
                VerifiedByAgent = claim.VerifiedByAgent != null 
                    ? $"{claim.VerifiedByAgent.FirstName} {claim.VerifiedByAgent.LastName}" 
                    : "N/A",
                ApprovedByAdmin = claim.ApprovedByAdmin != null 
                    ? $"{claim.ApprovedByAdmin.FirstName} {claim.ApprovedByAdmin.LastName}" 
                    : "N/A",
                ClaimType = claim.ClaimType,
                IncidentDate = claim.IncidentDate,
                IncidentTime = claim.IncidentTime,
                IncidentLocation = claim.IncidentLocation,
                IncidentDescription = claim.IncidentDescription,
                ClaimAmountRequested = claim.ClaimAmountRequested,
                CustomerFullName = claim.Policy?.User != null 
                    ? $"{claim.Policy.User.FirstName} {claim.Policy.User.LastName}" 
                    : "N/A",
                CustomerEmail = claim.Policy?.User?.Email ?? "N/A",
                CustomerUserId = claim.Policy?.UserId ?? "N/A",
                UploadedDocuments = claim.Documents?.ToList() ?? new List<ClaimDocument>(),
            };

            model.AvailableAgents = await GetAgentsList();

            return View(model);
        }

        private async Task<List<SelectListItem>> GetAgentsList()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.AGENT)
                .Select(a => new SelectListItem 
                {
                    Value = a.Id,
                    Text = $"{a.FirstName} {a.LastName}"
                })
                .ToListAsync();
        }

        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ClaimStatus status) // Changed parameter to "id"
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claim = await _claimService.UpdateClaimStatusAsync(id, status, adminId); // Use "id"

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Failed to update claim status.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Claim status updated to {status} successfully!";
            }

            return RedirectToAction("Details", new { id }); // Use "id"
        }

       [HttpPost("AssignAgent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAgent(int id, string agentId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.VerifiedByAgentId = agentId;
            claim.Status = ClaimStatus.UnderReview;
            claim.DateVerified = DateTime.UtcNow;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Agent assigned successfully!";
            return RedirectToAction("Details", new { id });
        }
    }
}