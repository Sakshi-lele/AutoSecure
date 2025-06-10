// File: Models/Policy.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [Column] if needed, but not strictly for DateCreated

namespace Auto_Insurance_Management_System.Models
{
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required]
        [StringLength(50)]
        public string PolicyNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string VehicleDetails { get; set; } // e.g., Make, Model, Year, VIN
        public string VehicleMake { get; set; }

        public string VehicleModel { get; set; }

        public int VehicleYear { get; set; }

        public string LicensePlate { get; set; }

        [Required]
        [StringLength(50)]
        public string CoverageType { get; set; } // e.g., "Full Coverage", "Liability Only"

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Example for currency
        public decimal CoverageAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Example for currency
        public decimal PremiumAmount { get; set; }

        [Required]
        [DataType(DataType.Date)] // Specifies it's a date only
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)] // Specifies it's a date only
        public DateTime EndDate { get; set; }

       
        [StringLength(20)]
        public string PolicyStatus { get; set; } // e.g., "ACTIVE", "INACTIVE", "RENEWED"

        // *** THIS IS THE CRITICAL MISSING COLUMN ***
        [Required] // If it must always have a value
        [DataType(DataType.DateTime)] // Specifies it's a date and time
        public DateTime DateCreated { get; set; }

        // Foreign Key to link with the user (AspNetUsers)
        // Assuming your ApplicationUser has an 'Id' property of type string
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Navigation property to the user

        // Navigation property for Claims (if one-to-many relationship)
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }

    // You likely already have this, but confirm your ApplicationUser model
    // is inheriting from IdentityUser
    // File: Models/ApplicationUser.cs
    // using Microsoft.AspNetCore.Identity;
    // namespace Auto_Insurance_Management_System.Models
    // {
    //     public class ApplicationUser : IdentityUser
    //     {
    //         // Add any custom properties like FirstName, LastName, etc.
    //         public string FirstName { get; set; }
    //         public string LastName { get; set; }
    //         public DateTime? LastLoginAt { get; set; }
    //         public bool IsActive { get; set; }
    //         public DateTime CreatedAt { get; set; }
    //         public string Role { get; set; } // Consider using Identity Roles for better practice
    //     }
    // }
}