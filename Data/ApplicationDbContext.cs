// File: Data/ApplicationDbContext.cs
using Auto_Insurance_Management_System.Models; // Make sure this namespace is correct and contains User, Policy, Claim
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
// using System.Linq; // Not strictly needed for this snippet, can keep or remove based on other code
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // <<< Keep this line!

namespace Auto_Insurance_Management_System.Data
{
    // Inherit from IdentityDbContext<User> as your custom user class is named 'User'
    public class ApplicationDbContext : IdentityDbContext<User> // CONFIRMED: Uses your 'User' class
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your DbSet for Policy and Claim entities
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Claim> Claims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.

            // --- IMPORTANT RELATIONSHIP CONFIGURATION ---

            // 1. Configure the relationship between Policy and User
            builder.Entity<Policy>()
                .HasOne(p => p.User)
                .WithMany(u => u.Policies)
                .HasForeignKey(p => p.UserId)
                .IsRequired();

            // 2. Configure the relationship between Claim and Policy
            builder.Entity<Claim>()
                .HasOne(c => c.Policy)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.PolicyId)
                .IsRequired();

            // 3. Configure the 'Role' property in your 'User' model
            // THIS IS THE CHANGE YOU NEED TO MAKE:
            builder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<UserRole>()); // <<< UNCOMMENT AND USE THIS LINE
            // This will store "ADMIN", "AGENT", "CUSTOMER" strings in the DB column.

            // The commented-out line below (HasConversion<string>()) also works,
            // but EnumToStringConverter is generally preferred for clarity and directness
            // when dealing with enums.
            // builder.Entity<User>()
            //    .Property(u => u.Role)
            //    .HasConversion<string>();
        }
    }
}