// In IPolicyService.cs (likely in your Services folder)

using Auto_Insurance_Management_System.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public interface IPolicyService
    {
        // Modified to accept search and status parameters
        Task<List<PolicyDetailsViewModel>> GetAllPoliciesAsync(string search, string status);
        // Modified to accept search and status parameters
        Task<List<PolicyDetailsViewModel>> GetPoliciesByUserIdAsync(string userId, string search, string status);

        Task<PolicyDetailsViewModel> GetPolicyByIdAsync(int id);
        Task<bool> CreatePolicyAsync(CreatePolicyViewModel model, string userId);
        // Modified to accept processedBy parameter
        Task<bool> UpdatePolicyStatusAsync(int id, string status, string processedBy);
        Task<bool> UpdatePolicyAsync(int id, CreatePolicyViewModel model);
        Task<bool> DeletePolicyAsync(int id);

        Task<bool> CreateClaimAsync(CreateClaimViewModel model, string userId);
        Task<List<ClaimDetailsViewModel>> GetClaimsByUserIdAsync(string userId);
        Task<List<ClaimDetailsViewModel>> GetAllClaimsAsync();
        Task<bool> ProcessClaimAsync(int claimId, string status, string processedBy, string? rejectionReason);
    }
}