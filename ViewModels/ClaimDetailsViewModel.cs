using System;
using System.Collections.Generic;
using Auto_Insurance_Management_System.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auto_Insurance_Management_System.ViewModels
{
    public class ClaimDetailsViewModel
    {
        public int ClaimId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public DateTime PolicyStartDate { get; set; }
        public DateTime PolicyEndDate { get; set; }
        public ClaimStatus Status { get; set; }
        public DateTime DateOfSubmission { get; set; }
        public string? VerifiedByAgent { get; set; }
        public string? ApprovedByAdmin { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public TimeSpan IncidentTime { get; set; }
        public string LocationOfIncident { get; set; } = string.Empty;
        
        public string IncidentDescription { get; set; } = string.Empty;
        public decimal ClaimAmountRequested { get; set; }
        public DateTime? DateVerified { get; set; }
        public decimal? DamageEstimate { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerUserId { get; set; } = string.Empty;
        public List<SelectListItem> AvailableAgents { get; set; } = new List<SelectListItem>();
        public string IncidentLocation { get; set; }

        
        // Only keep one UploadedDocuments property
        public List<ClaimDocument> UploadedDocuments { get; set; } = new List<ClaimDocument>();
    }
}