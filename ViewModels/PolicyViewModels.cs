using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Required for attributes like [Required], [Range], [StringLength]
using System.ComponentModel.DataAnnotations.Schema; // Not strictly needed for ViewModels, but good to include if copying from Models

namespace Auto_Insurance_Management_System.ViewModels
{
    // ViewModel for displaying policy details
    public class PolicyDetailsViewModel
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; }
        public string UserName { get; set; } // Displays the user's name
        public string UserId { get; set; } // The ID of the user who owns the policy
        public string VehicleDetails { get; set; }
        public string CoverageType { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyStatus { get; set; }
        public DateTime DateCreated { get; set; } // Date when the policy was created
        public List<ClaimDetailsViewModel> Claims { get; set; } = new List<ClaimDetailsViewModel>(); // List of claims related to this policy
    }

    // ViewModel for creating and editing policies
    public class CreatePolicyViewModel
    {
        public int PolicyId { get; set; } // Used when editing an existing policy
        public string UserId { get; set; } // Could be used in admin contexts to assign policy to a specific user

        [Required(ErrorMessage = "Vehicle details are required.")]
        [StringLength(255, ErrorMessage = "Vehicle details cannot exceed 255 characters.")]
        public string VehicleDetails { get; set; }

        [Required(ErrorMessage = "Coverage type is required.")]
        [StringLength(50, ErrorMessage = "Coverage type cannot exceed 50 characters.")]
        public string CoverageType { get; set; }

        [Required(ErrorMessage = "Coverage amount is required.")]
        [Range(1000, 1000000, ErrorMessage = "Coverage amount must be between $1,000 and $1,000,000")]
        [DataType(DataType.Currency)]
        public decimal CoverageAmount { get; set; }

        [Required(ErrorMessage = "Premium amount is required.")]
        [Range(50, 10000, ErrorMessage = "Premium amount must be between $50 and $10,000")]
        [DataType(DataType.Currency)]
        public decimal PremiumAmount { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Additional properties if needed for admin/agent use, e.g., to assign a user
        // public string SelectedUserId { get; set; }
        // public IEnumerable<SelectListItem> UsersList { get; set; }
    }

    // ViewModel for displaying claim details
    public class ClaimDetailsViewModel
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } // For display purposes, to show associated policy number
        public string UserName { get; set; } // For display purposes, to show the user who filed the claim
        public string IncidentType { get; set; }
        public DateTime IncidentDate { get; set; }
        public string Description { get; set; }
        public decimal ClaimAmount { get; set; }
        public string ClaimStatus { get; set; }
        public string ProcessedBy { get; set; } // User who processed the claim
        public DateTime DateFiled { get; set; } // Date when the claim was filed
        public DateTime? DateProcessed { get; set; } // Date when the claim was processed (nullable)
        public string RejectionReason { get; set; }
    }

    // ViewModel for creating a new claim
    public class CreateClaimViewModel
    {
        [Required(ErrorMessage = "Policy ID is required.")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Incident type is required.")]
        [StringLength(100, ErrorMessage = "Incident type cannot exceed 100 characters.")]
        public string IncidentType { get; set; }

        [Required(ErrorMessage = "Incident date is required.")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Claim amount is required.")]
        [Range(1, 1000000, ErrorMessage = "Claim amount must be between $1 and $1,000,000.")]
        [DataType(DataType.Currency)]
        public decimal ClaimAmount { get; set; }
    }
}