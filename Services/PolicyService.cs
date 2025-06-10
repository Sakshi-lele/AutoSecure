// Auto_Insurance_Management_System/Services/PolicyService.cs

using Microsoft.EntityFrameworkCore;
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Auto_Insurance_Management_System.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implementation of GetAllPoliciesAsync
        public async Task<List<PolicyDetailsViewModel>> GetAllPoliciesAsync(string search, string status)
        {
            var query = _context.Policies
                                 .Include(p => p.User)
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
                // *** FIX: Map individual vehicle properties here ***
                VehicleMake = p.VehicleMake,
                VehicleModel = p.VehicleModel,
                VehicleYear = p.VehicleYear,
                LicensePlate = p.LicensePlate,
                VehicleDetails = p.VehicleDetails, // Keep this if you still use it elsewhere for combined display
                CoverageType = p.CoverageType,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PolicyStatus = p.PolicyStatus,
                DateCreated = p.DateCreated,
            }).ToListAsync();
        }

        // Implementation of GetPoliciesByUserIdAsync
        public async Task<List<PolicyDetailsViewModel>> GetPoliciesByUserIdAsync(string userId, string search, string status)
        {
            var query = _context.Policies
                                 .Where(p => p.UserId == userId)
                                 .Include(p => p.User)
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
                // *** FIX: Map individual vehicle properties here ***
                VehicleMake = p.VehicleMake,
                VehicleModel = p.VehicleModel,
                VehicleYear = p.VehicleYear,
                LicensePlate = p.LicensePlate,
                VehicleDetails = p.VehicleDetails, // Keep this if you still use it elsewhere for combined display
                CoverageType = p.CoverageType,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                PolicyStatus = p.PolicyStatus,
                DateCreated = p.DateCreated,
            }).ToListAsync();
        }

        // Implementation of GetPolicyByIdAsync
        public async Task<PolicyDetailsViewModel> GetPolicyByIdAsync(int id)
        {
            var policy = await _context.Policies
                                         .Include(p => p.User)
                                         .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy == null)
            {
                return null;
            }

            return new PolicyDetailsViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                UserName = policy.User?.UserName,
                UserId = policy.UserId,
                // *** FIX: Map individual vehicle properties here ***
                VehicleMake = policy.VehicleMake,
                VehicleModel = policy.VehicleModel,
                VehicleYear = policy.VehicleYear,
                LicensePlate = policy.LicensePlate,
                VehicleDetails = policy.VehicleDetails, // Keep this if you still use it elsewhere for combined display
                CoverageType = policy.CoverageType,
                CoverageAmount = policy.CoverageAmount,
                PremiumAmount = policy.PremiumAmount,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PolicyStatus = policy.PolicyStatus,
                DateCreated = policy.DateCreated,
            };
        }

        // Implementation of CreatePolicyAsync - This already looks correct for saving
        public async Task<bool> CreatePolicyAsync(CreatePolicyViewModel model, string userId)
        {
            var policy = new Policy
            {
                PolicyNumber = model.PolicyNumber ?? Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserId = userId,
                VehicleMake = model.VehicleMake,
                VehicleModel = model.VehicleModel,
                VehicleYear = model.VehicleYear,
                LicensePlate = model.LicensePlate,
                VehicleDetails = $"{model.VehicleYear} {model.VehicleMake} {model.VehicleModel} ({model.LicensePlate})", // Combine details
                CoverageAmount = model.CoverageAmount,
                CoverageType = model.CoverageType,
                PremiumAmount = model.PremiumAmount,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PolicyStatus = "ACTIVE",
                DateCreated = DateTime.UtcNow
            };

            _context.Policies.Add(policy);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        // Implementation of UpdatePolicyStatusAsync - No changes needed here
        public async Task<bool> UpdatePolicyStatusAsync(int id, string status, string processedBy)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null)
            {
                return false;
            }
            policy.PolicyStatus = status;
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementation of UpdatePolicyAsync - This already looks correct for saving
        public async Task<bool> UpdatePolicyAsync(int id, CreatePolicyViewModel model)
        {
            var existingPolicy = await _context.Policies.FindAsync(id);
            if (existingPolicy == null)
            {
                return false;
            }

            existingPolicy.PolicyNumber = model.PolicyNumber;
            existingPolicy.VehicleMake = model.VehicleMake;
            existingPolicy.VehicleModel = model.VehicleModel;
            existingPolicy.VehicleYear = model.VehicleYear;
            existingPolicy.LicensePlate = model.LicensePlate;
            existingPolicy.VehicleDetails = $"{model.VehicleYear} {model.VehicleMake} {model.VehicleModel} ({model.LicensePlate})";
            existingPolicy.CoverageAmount = model.CoverageAmount;
            existingPolicy.CoverageType = model.CoverageType;
            existingPolicy.PremiumAmount = model.PremiumAmount;
            existingPolicy.StartDate = model.StartDate;
            existingPolicy.EndDate = model.EndDate;
            existingPolicy.PolicyStatus = "ACTIVE"; // Consider if status should always be active on update or derived

            _context.Policies.Update(existingPolicy);
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        // Implementation of DeletePolicyAsync - No changes needed here
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

        // Implementation of ProcessPolicylifecycleAsync - No changes needed here
        public async Task<bool> ProcessPolicylifecycleAsync(int policyId)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(policyId);
                if (policy == null)
                {
                    Console.WriteLine($"Policy with ID {policyId} not found for lifecycle processing.");
                    return false;
                }

                if (policy.EndDate < DateTime.UtcNow && policy.PolicyStatus == "ACTIVE")
                {
                    policy.PolicyStatus = "EXPIRED";
                    Console.WriteLine($"Policy {policy.PolicyNumber} (ID: {policyId}) has expired.");
                }

                _context.Policies.Update(policy);
                var saved = await _context.SaveChangesAsync();

                if (saved > 0)
                {
                    Console.WriteLine($"Policy {policy.PolicyNumber} (ID: {policyId}) lifecycle processed successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Policy {policy.PolicyNumber} (ID: {policyId}) lifecycle processing resulted in no changes.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in ProcessPolicylifecycleAsync for Policy ID {policyId}: {ex.Message}");
                return false;
            }
        }
    }
}