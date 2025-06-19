// Services/ClaimService.cs
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Auto_Insurance_Management_System.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Claim> CreateClaimAsync(QuickClaimFormViewModel model, string userId)
        {
            var policy = await _context.Policies
                .FirstOrDefaultAsync(p => p.PolicyNumber == model.SelectedPolicyNumber && p.UserId == userId);

            if (policy == null) return null;

            var claim = new Claim
            {
                PolicyId = policy.Id,
                ClaimType = model.ClaimType,
                OtherClaimType = model.OtherClaimType,
                IncidentDate = model.IncidentDate,
                IncidentTime = model.IncidentTime,
                IncidentLocation = model.IncidentLocation,
                IncidentDescription = model.IncidentDescription,
                ClaimAmountRequested = model.ClaimAmountRequested,
                Status = ClaimStatus.Submitted,
                DateOfSubmission = DateTime.UtcNow
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            if (model.UploadedFiles != null && model.UploadedFiles.Count > 0)
            {
                await SaveClaimDocumentsAsync(claim.ClaimId, model.UploadedFiles);
            }

            return claim;
        }

        public async Task SaveClaimDocumentsAsync(int claimId, List<IFormFile> files)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "claims");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var document = new ClaimDocument
                    {
                        ClaimId = claimId,
                        FileName = file.FileName,
                        FilePath = $"/uploads/claims/{uniqueFileName}"
                    };

                    _context.ClaimDocuments.Add(document);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Claim>> GetClaimsByUserIdAsync(string userId)
{
    return await _context.Claims
        .Include(c => c.Policy)
        .Where(c => c.Policy.UserId == userId) // Filter by Policy.UserId
        .ToListAsync();
}

public async Task<Claim> GetClaimByIdAsync(int claimId)
{
    return await _context.Claims
        .Include(c => c.Policy)
            .ThenInclude(p => p.User) // Include user through Policy
        // ... other includes ...
        .FirstOrDefaultAsync(c => c.ClaimId == claimId);
}

public async Task<Claim> GetClaimByIdWithDetailsAsync(int ClaimId)
{
    return await _context.Claims
        .Include(c => c.Policy)
            .ThenInclude(p => p.User)
        .Include(c => c.Documents)
        .FirstOrDefaultAsync(c => c.ClaimId == ClaimId);
}
public async Task<Claim> UpdateClaimStatusAsync(int claimId, ClaimStatus status, string processedById)
{
    var claim = await _context.Claims
        .Include(c => c.Policy)
        .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        
    if (claim == null) return null;

    claim.Status = status;
    claim.VerifiedByAgentId = processedById;
    claim.DateVerified = DateTime.UtcNow;

    try
    {
        await _context.SaveChangesAsync();
        return claim;
    }
    catch
    {
        return null;
    }
}

        // public async Task<Claim> UpdateClaimStatusAsync(int claimId, ClaimStatus status, string processedById)
        // {
        //     var claim = await _context.Claims.FindAsync(claimId);
        //     if (claim == null) return null;

        //     claim.Status = status;

        //     if (status == ClaimStatus.UnderReview || status == ClaimStatus.Rejected)
        //     {
        //         claim.VerifiedByAgentId = processedById;
        //         claim.DateVerified = DateTime.UtcNow;
        //     }
        //     else if (status == ClaimStatus.Approved || status == ClaimStatus.Settled)
        //     {
        //         claim.ApprovedByAdminId = processedById;
        //         claim.DateApproved = DateTime.UtcNow;
        //     }

        //     _context.Claims.Update(claim);
        //     await _context.SaveChangesAsync();
        //     return claim;
        // }

        public async Task<List<User>> GetAgentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.AGENT)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
{
    return await _context.Claims
        .Include(c => c.Policy)
        .Include(c => c.Policy.User)
        .OrderByDescending(c => c.DateOfSubmission)
        .ToListAsync();
}

        public async Task<List<Claim>> GetClaimsForAgentAsync(string agentId)
        {
            return await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.Policy.User)
                .Where(c => c.VerifiedByAgentId == agentId || 
                            (c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Open))
                .OrderByDescending(c => c.DateOfSubmission)
                .ToListAsync();
        }
        
        public async Task<int> GetPendingClaimsCountAsync()
        {
            return await _context.Claims
                .CountAsync(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Open);
        }
        
        public async Task<int> GetResolvedClaimsCountAsync()
        {
            return await _context.Claims
                .CountAsync(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Settled);
        }
    }
}