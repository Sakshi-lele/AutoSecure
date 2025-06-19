using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace Auto_Insurance_Management_System.Models
{
    public enum TicketStatus
    {
        [Description("Open")]
        Open,
        [Description("In Progress")]
        InProgress,
        [Description("Resolved")]
        Resolved,
        [Description("Closed")]
        Closed
    }

    public enum QueryType
    {
        [Description("Billing Inquiry")]
        Billing,
        [Description("Claim Assistance")]
        Claim,
        [Description("Policy Change")]
        PolicyChange,
        [Description("Cancellation Request")]
        Cancellation,
        [Description("Other")]
        Other
    }

    public class SupportTicket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [ForeignKey("Policy")]
        [Display(Name = "Policy")]
        public int PolicyId { get; set; }

        // Remove validation attributes from navigation properties
        public virtual Policy Policy { get; set; }

        [Required]
        [ForeignKey("User")]
        [Display(Name = "Customer")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [Required]
        [Display(Name = "Query Type")]
        public QueryType QueryType { get; set; } = QueryType.Other;

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Status")]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public virtual ICollection<TicketResponse> Responses { get; set; } = new HashSet<TicketResponse>();
    }

    public class TicketResponse
{
    [Key]
    public int ResponseId { get; set; }

    [Required]
    [ForeignKey("SupportTicket")]
    public int TicketId { get; set; }
    public virtual SupportTicket SupportTicket { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }
    public virtual User User { get; set; }

    [Required(ErrorMessage = "Response content is required")]
    [StringLength(1000, ErrorMessage = "Response cannot exceed 1000 characters")]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Is Internal Note")]
    public bool IsInternalNote { get; set; } = false;
}
}