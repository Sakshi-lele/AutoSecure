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

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketResponse> TicketResponses { get; set; }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }

        public DbSet<Payment> Payments { get; set; }

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

            builder.Entity<SupportTicket>()
                .HasOne(st => st.Policy)
                .WithMany(p => p.SupportTickets)
                .HasForeignKey(st => st.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupportTicket>()
                .HasOne(st => st.User)
                .WithMany(u => u.SupportTickets)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Policy>()
                .HasMany(p => p.Claims)
                .WithOne(c => c.Policy)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Claim>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

                  builder.Entity<Claim>()
        .HasOne(c => c.Policy)
        .WithMany(p => p.Claims)
        .HasForeignKey(c => c.PolicyId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Add explicit Policy-User relationship
    builder.Entity<Policy>()
        .HasOne(p => p.User)
        .WithMany(u => u.Policies)
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Restrict);

                  builder.Entity<Claim>()
        .HasOne(c => c.VerifiedByAgent)
        .WithMany() // No inverse navigation
        .HasForeignKey(c => c.VerifiedByAgentId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Claim>()
        .HasOne(c => c.ApprovedByAdmin)
        .WithMany() // No inverse navigation
        .HasForeignKey(c => c.ApprovedByAdminId)
        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}