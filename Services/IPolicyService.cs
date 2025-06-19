using Auto_Insurance_Management_System.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public interface IPolicyService
    {
        Task<List<PolicyDetailsViewModel>> GetAllPoliciesAsync(string search, string status);
        Task<List<PolicyDetailsViewModel>> GetPoliciesByUserIdAsync(string userId, string search, string status);
        Task<PolicyDetailsViewModel> GetPolicyByPolicyNumberAsync(string policyNumber);
        Task<PolicyDetailsViewModel> GetPolicyByIdAsync(int id);
        Task<bool> CreatePolicyAsync(CreatePolicyViewModel model, string userId);
        Task<bool> UpdatePolicyStatusAsync(int id, string status, string processedBy);
        Task<bool> UpdatePolicyAsync(int id, CreatePolicyViewModel model);
        Task<bool> SoftDeletePolicyAsync(int id, string reason, string deletedByUserId);
        Task<List<PolicyDetailsViewModel>> GetDeletedPoliciesAsync();
        Task<bool> RestorePolicyAsync(int id);
        Task<bool> ProcessPolicylifecycleAsync(int policyId);
    }
}