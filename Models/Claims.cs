// In Models/Claim.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Insurance_Management_System.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClaimNumber { get; set; }

        [Required]
        public int PolicyId { get; set; } // Foreign key to Policy
        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; } // Navigation property

        [Required]
        [StringLength(100)]
        public string IncidentType { get; set; } // e.g., "Accident", "Theft", "Natural Disaster"

        [Required]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ClaimAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string ClaimStatus { get; set; } // e.g., "Pending", "Approved", "Rejected", "Paid"

        public string? ProcessedBy { get; set; } // User who processed the claim
        public string? RejectionReason { get; set; } // Reason if claim is rejected

        // ADD THIS LINE
        public DateTime DateFiled { get; set; }

        public DateTime? DateProcessed { get; set; } // Nullable, as it might not be processed yet
    }
}