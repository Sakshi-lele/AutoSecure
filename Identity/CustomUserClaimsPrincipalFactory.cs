// File: Auto_Insurance_Management_System/Identity/CustomUserClaimsPrincipalFactory.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims; // Still need this for ClaimTypes
using System.Threading.Tasks;

// Make sure this using directive points to where your 'User' model and 'UserRole' enum are defined.
using Auto_Insurance_Management_System.Models;

// --- ADD THIS NAMESPACE BLOCK ---
namespace Auto_Insurance_Management_System.Identity // <--- THIS IS THE MISSING PIECE!
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
    {
        // Constructor: Dependency Injection will provide UserManager, RoleManager, and IdentityOptions
        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        // Override this method to add custom claims
        public override async Task<System.Security.Claims.ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (System.Security.Claims.ClaimsIdentity)principal.Identity;

            if (user.Role != null)
            {
                identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()));
            }

            return principal;
        }
    }
}
// --- END NAMESPACE BLOCK ---