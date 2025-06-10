// File: Models/User.cs
using System; // <--- !!! ADD THIS LINE !!! Required for DateTime
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Make sure this is included for ICollection

namespace Auto_Insurance_Management_System.Models
{
    public enum UserRole
    {
        ADMIN,
        AGENT,
        CUSTOMER
    }

    // Your custom Identity user class is named 'User' (singular).
    // This is important! Ensure all references to your custom user class
    // throughout your application (DbContext, Program.cs, Controllers, etc.)
    // use 'User' instead of 'ApplicationUser' or 'Users'.
    public class User : IdentityUser
    {
        [Required]
        public UserRole Role { get; set; } // Will be stored as an integer in the DB

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Using UtcNow is generally good practice

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Excellent! These navigation properties correctly link User to Policies and Claims.
        public ICollection<Policy> Policies { get; set; }
        //public ICollection<Claim> Claims { get; set; }

        // Good practice to initialize collections in the constructor to avoid NullReferenceExceptions
        public User()
        {
            Policies = new HashSet<Policy>(); // Using HashSet is often more performant for collections where order doesn't matter and you need unique items
            //Claims = new HashSet<Claim>();
        }
    }
}