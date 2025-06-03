// In PolicyService.cs (likely in your Services folder)

using Auto_Insurance_Management_System.ViewModels;
using Auto_Insurance_Management_System.Models; // Assuming your Policy and Claim models are here
using Microsoft.EntityFrameworkCore; // If you are using Entity Framework
using System.Collections.Generic;
using System.Linq; // For LINQ extension methods
using System.Threading.Tasks;
using System; // For Guid and DateTime
using Auto_Insurance_Management_System.Data;
namespace Auto_Insurance_Management_System.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context; // Assuming you have a DbContext

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implementation of GetAllPoliciesAsync (as provided in previous corrected code)
        public async Task<List<PolicyDetailsViewModel>> GetAllPoliciesAsync(string search, string status)
        {
            var query = _context.Policies
                                .Include(p => p.User)
                                .Include(p => p.Claims)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.PolicyNumber.Contains(search) ||
                                         p.User.UserName.Contains(search) ||
                                         p.VehicleDetails.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.PolicyStatus == status);
            }

            return await query.Select(p => new PolicyDetailsViewModel
            {
                PolicyId = p.PolicyId,
                PolicyNumber = p.PolicyNumber,
                UserName = p.User.UserName,
                UserId = p.User.Id,
                VehicleDetails = p.VehicleDetails,
                CoverageType = p.CoverageType,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PolicyStatus = p.PolicyStatus,
                DateCreated = p.DateCreated, // Ensure this maps from your Policy model
                Claims = p.Claims.Select(c => new ClaimDetailsViewModel
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    // ... other claim properties
                }).ToList()
            }).ToListAsync();
        }

        // Implementation of GetPoliciesByUserIdAsync (as provided in previous corrected code)
        public async Task<List<PolicyDetailsViewModel>> GetPoliciesByUserIdAsync(string userId, string search, string status)
        {
            var query = _context.Policies
                                .Where(p => p.UserId == userId)
                                .Include(p => p.User)
                                .Include(p => p.Claims)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.PolicyNumber.Contains(search) ||
                                         p.VehicleDetails.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.PolicyStatus == status);
            }

            return await query.Select(p => new PolicyDetailsViewModel
            {
                PolicyId = p.PolicyId,
                PolicyNumber = p.PolicyNumber,
                UserName = p.User.UserName,
                UserId = p.User.Id,
                VehicleDetails = p.VehicleDetails,
                CoverageType = p.CoverageType,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PolicyStatus = p.PolicyStatus,
                DateCreated = p.DateCreated, // Ensure this maps from your Policy model
                Claims = p.Claims.Select(c => new ClaimDetailsViewModel
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    // ... other claim properties
                }).ToList()
            }).ToListAsync();
        }

        // Fix for CS0535: GetPolicyByIdAsync
        public async Task<PolicyDetailsViewModel> GetPolicyByIdAsync(int id)
        {
            var policy = await _context.Policies
                                       .Include(p => p.User)
                                       .Include(p => p.Claims)
                                       .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy == null)
            {
                return null;
            }

            return new PolicyDetailsViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                UserName = policy.User?.UserName, // Null-conditional operator for safety
                UserId = policy.UserId,
                VehicleDetails = policy.VehicleDetails,
                CoverageType = policy.CoverageType,
                CoverageAmount = policy.CoverageAmount,
                PremiumAmount = policy.PremiumAmount,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PolicyStatus = policy.PolicyStatus,
                DateCreated = policy.DateCreated, // Ensure this maps from your Policy model
                Claims = policy.Claims?.Select(c => new ClaimDetailsViewModel
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    IncidentDate = c.IncidentDate,
                    Description = c.Description,
                    ClaimAmount = c.ClaimAmount,
                    ClaimStatus = c.ClaimStatus,
                    ProcessedBy = c.ProcessedBy,
                    DateFiled = c.DateFiled, // Ensure this maps from your Claim model
                    DateProcessed = c.DateProcessed,
                    RejectionReason = c.RejectionReason
                }).ToList()
            };
        }

        // Implementation of CreatePolicyAsync (as provided in previous corrected code)
        public async Task<bool> CreatePolicyAsync(CreatePolicyViewModel model, string userId)
        {
            var policy = new Policy
            {
                PolicyNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserId = userId,
                VehicleDetails = model.VehicleDetails,
                CoverageAmount = model.CoverageAmount,
                CoverageType = model.CoverageType,
                PremiumAmount = model.PremiumAmount,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PolicyStatus = "ACTIVE",
                DateCreated = DateTime.UtcNow // Ensure this property exists in your Policy model
            };

            _context.Policies.Add(policy);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        // Fix for CS0535: UpdatePolicyStatusAsync (as provided in previous corrected code)
        public async Task<bool> UpdatePolicyStatusAsync(int id, string status, string processedBy)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
            {
                return false;
            }
            policy.PolicyStatus = status;
            // You might want to add a field like 'LastUpdatedBy' or 'ProcessedBy' to your Policy model
            // policy.LastUpdatedBy = processedBy;
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementation of UpdatePolicyAsync (as provided in previous corrected code)
        public async Task<bool> UpdatePolicyAsync(int id, CreatePolicyViewModel model)
        {
            var existingPolicy = await _context.Policies.FindAsync(id);
            if (existingPolicy == null)
            {
                return false;
            }

            existingPolicy.VehicleDetails = model.VehicleDetails;
            existingPolicy.CoverageAmount = model.CoverageAmount;
            existingPolicy.CoverageType = model.CoverageType;
            existingPolicy.PremiumAmount = model.PremiumAmount;
            existingPolicy.StartDate = model.StartDate;
            existingPolicy.EndDate = model.EndDate;
            // existingPolicy.UserId = model.UserId; // Only update if you intend to allow changing policy owner

            _context.Policies.Update(existingPolicy);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        // Fix for CS0535: DeletePolicyAsync
        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
            {
                return false;
            }

            _context.Policies.Remove(policy);
            var deleted = await _context.SaveChangesAsync();
            return deleted > 0;
        }

        // Fix for CS0535: CreateClaimAsync (as provided in previous corrected code)
        public async Task<bool> CreateClaimAsync(CreateClaimViewModel model, string userId)
        {
            var policy = await _context.Policies.FindAsync(model.PolicyId);
            if (policy == null || policy.UserId != userId)
            {
                return false; // Policy not found or doesn't belong to user
            }

            var claim = new Claim
            {
                PolicyId = model.PolicyId,
                ClaimNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                IncidentDate = model.IncidentDate,
                Description = model.Description,
                ClaimAmount = model.ClaimAmount, // Added ClaimAmount as it's in CreateClaimViewModel
                ClaimStatus = "PENDING",
                DateFiled = DateTime.UtcNow // Ensure this property exists in your Claim model
            };

            _context.Claims.Add(claim);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        // Fix for CS0535: GetClaimsByUserIdAsync
        public async Task<List<ClaimDetailsViewModel>> GetClaimsByUserIdAsync(string userId)
        {
            return await _context.Claims
                                 .Where(c => c.Policy.UserId == userId) // Assuming Policy has a User property or UserId directly on Claim
                                 .Include(c => c.Policy) // Include policy to get PolicyNumber
                                 .Select(c => new ClaimDetailsViewModel
                                 {
                                     ClaimId = c.ClaimId,
                                     ClaimNumber = c.ClaimNumber,
                                     PolicyId = c.PolicyId,
                                     PolicyNumber = c.Policy.PolicyNumber,
                                     IncidentType = c.IncidentType, // Assuming you have this in your Claim model
                                     IncidentDate = c.IncidentDate,
                                     Description = c.Description,
                                     ClaimAmount = c.ClaimAmount,
                                     ClaimStatus = c.ClaimStatus,
                                     ProcessedBy = c.ProcessedBy,
                                     DateFiled = c.DateFiled, // Ensure this maps from your Claim model
                                     DateProcessed = c.DateProcessed,
                                     RejectionReason = c.RejectionReason
                                 })
                                 .ToListAsync();
        }

        // Fix for CS0535: GetAllClaimsAsync
        public async Task<List<ClaimDetailsViewModel>> GetAllClaimsAsync()
        {
            return await _context.Claims
                                 .Include(c => c.Policy) // Include policy to get PolicyNumber
                                 .Include(c => c.Policy.User) // Include user to get UserName
                                 .Select(c => new ClaimDetailsViewModel
                                 {
                                     ClaimId = c.ClaimId,
                                     ClaimNumber = c.ClaimNumber,
                                     PolicyId = c.PolicyId,
                                     PolicyNumber = c.Policy.PolicyNumber,
                                     UserName = c.Policy.User.UserName, // Map user's name
                                     IncidentType = c.IncidentType, // Assuming you have this in your Claim model
                                     IncidentDate = c.IncidentDate,
                                     Description = c.Description,
                                     ClaimAmount = c.ClaimAmount,
                                     ClaimStatus = c.ClaimStatus,
                                     ProcessedBy = c.ProcessedBy,
                                     DateFiled = c.DateFiled, // Ensure this maps from your Claim model
                                     DateProcessed = c.DateProcessed,
                                     RejectionReason = c.RejectionReason
                                 })
                                 .ToListAsync();
        }

        // Fix for CS0535: ProcessClaimAsync (as provided in previous corrected code)
        public async Task<bool> ProcessClaimAsync(int claimId, string status, string processedBy, string? rejectionReason)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return false;
            }

            claim.ClaimStatus = status;
            claim.ProcessedBy = processedBy;
            claim.DateProcessed = DateTime.UtcNow;
            claim.RejectionReason = (status == "Rejected") ? rejectionReason : null;

            _context.Claims.Update(claim);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}