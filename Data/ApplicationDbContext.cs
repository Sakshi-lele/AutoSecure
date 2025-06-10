// Auto_Insurance_Management_System/Data/ApplicationDbContext.cs

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auto_Insurance_Management_System.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // <--- ADD THIS USING DIRECTIVE

namespace Auto_Insurance_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Policy> Policies { get; set; }
        // public DbSet<Claim> Claims { get; set; } // Keep if you have a Claim model

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Add this configuration to tell EF Core to store the UserRole enum as a string
            builder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<UserRole>()); // <--- ADD THIS LINE

            // Ensure other configurations are still present:
            builder.Entity<Policy>()
                .HasOne(p => p.User)
                .WithMany(u => u.Policies)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}