// Services/IClaimService.cs
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public interface IClaimService
    {
        Task<Claim> CreateClaimAsync(QuickClaimFormViewModel model, string userId);
        Task<List<Claim>> GetClaimsByUserIdAsync(string userId);
        Task<Claim> GetClaimByIdAsync(int claimId);
        Task<Claim> UpdateClaimStatusAsync(int claimId, ClaimStatus status, string processedById);
        Task SaveClaimDocumentsAsync(int claimId, List<IFormFile> files);
        Task<List<User>> GetAgentsAsync();
        Task<List<Claim>> GetAllClaimsAsync();
        Task<List<Claim>> GetClaimsForAgentAsync(string agentId);
        Task<int> GetPendingClaimsCountAsync();
        Task<int> GetResolvedClaimsCountAsync();
        Task<Claim> GetClaimByIdWithDetailsAsync(int ClaimId);
    }
}