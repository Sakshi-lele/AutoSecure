using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Auto_Insurance_Management_System.ViewModels
{
    // ViewModel for displaying policy details on the Details page/view
    public class PolicyDetailsViewModel
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; }
        public string UserName { get; set; } // Displays the user's full name
        public string UserId { get; set; } // The ID of the user who owns the policy

        // **CRITICAL FIX for CS1061 in Details.cshtml:**
        // These properties were missing and are needed to display vehicle information.
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public int VehicleYear { get; set; }
        public string LicensePlate { get; set; }

        public string VehicleDetails { get; set; } // This is likely a combined string for display
        public string CoverageType { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; } // Also confirmed for display
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyStatus { get; set; } // Confirmed for display
        public DateTime DateCreated { get; set; } // Date when the policy was created
    }

    // ViewModel for creating and editing policies (used for input forms)
    public class CreatePolicyViewModel
    {
        public int PolicyId { get; set; } // Used when editing an existing policy

        [Required(ErrorMessage = "Please select a customer for this policy.")]
        public string UserId { get; set; } // To bind the selected customer's ID from the dropdown

        [Required(ErrorMessage = "Policy Number is required.")]
        [StringLength(50, ErrorMessage = "Policy Number cannot exceed 50 characters.")]
        public string PolicyNumber { get; set; }

        [Required(ErrorMessage = "Vehicle make is required.")]
        [StringLength(100, ErrorMessage = "Vehicle make cannot exceed 100 characters.")]
        public string VehicleMake { get; set; }

        [Required(ErrorMessage = "Vehicle model is required.")]
        [StringLength(100, ErrorMessage = "Vehicle model cannot exceed 100 characters.")]
        public string VehicleModel { get; set; }

        [Required(ErrorMessage = "Vehicle year is required.")]
        [Range(1900, 2050, ErrorMessage = "Vehicle year must be between 1900 and 2050.")]
        public int VehicleYear { get; set; }

        [Required(ErrorMessage = "License plate is required.")]
        [StringLength(20, ErrorMessage = "License plate cannot exceed 20 characters.")]
        public string LicensePlate { get; set; }

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

        [Display(Name = "Policy Status")]
        public string Status { get; set; } // This corresponds to the PolicyStatus string in the Policy model
    }
}