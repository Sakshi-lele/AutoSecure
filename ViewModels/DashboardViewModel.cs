using System.Collections.Generic;
using Auto_Insurance_Management_System.Models; // For User and UserRole
using Auto_Insurance_Management_System.ViewModels; // For PolicyDetailsViewModel

namespace Auto_Insurance_Management_System.ViewModels
{
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; }
        public List<PolicyDetailsViewModel> AllPolicies { get; set; } = new List<PolicyDetailsViewModel>(); // Initialize to prevent null reference

        // You can add other dashboard-specific data here if needed (e.g., quick stats)
        // public int ActivePoliciesCount { get; set; }
        // public int PendingClaimsCount { get; set; }
    }
}