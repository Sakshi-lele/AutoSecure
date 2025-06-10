// File: Auto_Insurance_Management_System/Models/NotificationLog.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auto_Insurance_Management_System.Models
{
    public class NotificationLog
    {
        [Key]
        public int NotificationLogId { get; set; }

        public int PolicyId { get; set; } // Foreign Key to Policy
        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; }

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } // e.g., "RenewalReminder", "ExpirationNotice"

        [Required]
        public DateTime SentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // e.g., "Sent", "Failed"

        [StringLength(255)]
        public string RecipientInfo { get; set; } // e.g., email address, phone number

        public string MessageDetails { get; set; } // Optional: Store the full message content
    }
}