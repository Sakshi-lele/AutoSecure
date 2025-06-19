// Models/Claim.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Insurance_Management_System.Models
{

    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [ForeignKey("Policy")]
        public int PolicyId { get; set; }
        public virtual Policy Policy { get; set; }

        [Required]
        public string ClaimType { get; set; } = "Accident";

        [StringLength(50)]
        public string? OtherClaimType { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public TimeSpan IncidentTime { get; set; }

        [Required]
        [StringLength(255)]
        public string IncidentLocation { get; set; } = string.Empty;

        

        [Required]
        public string IncidentDescription { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ClaimAmountRequested { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? DamageEstimate { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        public DateTime DateOfSubmission { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();

        // Agent actions
        public string? VerifiedByAgentId { get; set; }
        [ForeignKey("VerifiedByAgentId")]
        public virtual User? VerifiedByAgent { get; set; }
        public DateTime? DateVerified { get; set; }

        // Admin actions
        public string? ApprovedByAdminId { get; set; }
        [ForeignKey("ApprovedByAdminId")]
        public virtual User? ApprovedByAdmin { get; set; }
        public DateTime? DateApproved { get; set; }

    }
}