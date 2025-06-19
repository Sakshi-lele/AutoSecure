// ViewModels/QuickClaimFormViewModel.cs
using Microsoft.AspNetCore.Http;  
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Auto_Insurance_Management_System.ViewModels
{
    public class QuickClaimFormViewModel
    {
        public string UserId { get; set; } = string.Empty;

        // Policy Details
        [Required(ErrorMessage = "Please select a policy")]
        [Display(Name = "Policy Number")]
        public string SelectedPolicyNumber { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Policies { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Vehicle number is required")]
        [Display(Name = "Vehicle Number")]
        public string VehicleNumber { get; set; } = string.Empty;

        [Display(Name = "Policy Start Date")]
        [DataType(DataType.Date)]
        public DateTime PolicyStartDate { get; set; }

        [Display(Name = "Policy End Date")]
        [DataType(DataType.Date)]
        public DateTime PolicyEndDate { get; set; }

        // Claim Details
        [Required(ErrorMessage = "Claim type is required")]
        [Display(Name = "Claim Type")]
        public string ClaimType { get; set; } = "Accident";

        [Display(Name = "Other Claim Type")]
        public string? OtherClaimType { get; set; }

        [Required(ErrorMessage = "Incident date is required")]
        [Display(Name = "Incident Date")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Incident time is required")]
        [Display(Name = "Incident Time")]
        [DataType(DataType.Time)]
        public TimeSpan IncidentTime { get; set; } = TimeSpan.Zero;

        [Required(ErrorMessage = "Incident location is required")]
        [Display(Name = "Incident Location")]
        public string IncidentLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident description is required")]
        [Display(Name = "Incident Description")]
        public string IncidentDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Claim amount is required")]
        [Display(Name = "Claim Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal ClaimAmountRequested { get; set; }

        [Display(Name = "Upload Documents")]
        public List<IFormFile>? UploadedFiles { get; set; } 
    }
}