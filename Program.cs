using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models; // IMPORTANT: This brings in 'User' and 'UserRole'
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.Identity; // Assuming CustomUserClaimsPrincipalFactory is here
using Microsoft.AspNetCore.Authentication.Cookies;
using Auto_Insurance_Management_System.BackgroundServices; // For your background service
using System; // For TimeSpan
using Microsoft.Extensions.Logging; // Required for ILogger

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **CRITICAL FIX: Use 'User' (singular) for Identity configuration**
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>(); // This is good for custom claims

// Configure the Identity cookie to redirect to your AuthController for AccessDenied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

// Add Support Service Registration (NEW)
builder.Services.AddScoped<ISupportService, SupportService>();

// Register your background service here
builder.Services.AddHostedService<PolicyLifecycleBackgroundService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Needed for Identity UI pages

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Consider adding app.UseMigrationsEndPoint(); if you want specific migration error pages
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.MapRazorPages(); // Maps Identity UI routes (e.g., /Identity/Account/Login)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- START: Database Seeding Logic (Runs on application startup) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // **CRITICAL FIX: Use 'User' (singular) for UserManager**
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        // Get an ILogger instance for logging seeding progress/errors
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database seeding...");

        // Ensure Roles exist as defined in your UserRole enum
        string[] roleNames = { nameof(UserRole.ADMIN), nameof(UserRole.AGENT), nameof(UserRole.CUSTOMER) };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation($"Creating role: {roleName}");
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Admin User
        if (await userManager.FindByEmailAsync("admin@example.com") == null)
        {
            logger.LogInformation("Seeding Admin user...");
            // **CRITICAL FIX: Use 'User' (singular) for creating new user instances**
            var adminUser = new User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                Role = UserRole.ADMIN // Set the custom Role enum property
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // *** CHANGE THIS PASSWORD IN PRODUCTION ***
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, nameof(UserRole.ADMIN));
                logger.LogInformation("Admin user seeded successfully.");
            }
            else
            {
                logger.LogError($"Failed to seed Admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Seed Agent User
        if (await userManager.FindByEmailAsync("agent@example.com") == null)
        {
            logger.LogInformation("Seeding Agent user...");
            var agentUser = new User
            {
                UserName = "agent@example.com",
                Email = "agent@example.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Agent",
                Role = UserRole.AGENT // Set the custom Role enum property
            };
            var result = await userManager.CreateAsync(agentUser, "Agent@123"); // *** CHANGE THIS PASSWORD IN PRODUCTION ***
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agentUser, nameof(UserRole.AGENT));
                logger.LogInformation("Agent user seeded successfully.");
            }
            else
            {
                logger.LogError($"Failed to seed Agent user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Seed Customer Users
        if (await userManager.FindByEmailAsync("customer1@example.com") == null)
        {
            logger.LogInformation("Seeding Customer 1 user...");
            var customer1 = new User
            {
                UserName = "customer1@example.com",
                Email = "customer1@example.com",
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Doe",
                Role = UserRole.CUSTOMER // Set the custom Role enum property
            };
            var result = await userManager.CreateAsync(customer1, "Customer@123"); // *** CHANGE THIS PASSWORD IN PRODUCTION ***
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer1, nameof(UserRole.CUSTOMER));
                logger.LogInformation("Customer 1 user seeded successfully.");
            }
            else
            {
                logger.LogError($"Failed to seed Customer 1 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        if (await userManager.FindByEmailAsync("customer2@example.com") == null)
        {
            logger.LogInformation("Seeding Customer 2 user...");
            var customer2 = new User
            {
                UserName = "customer2@example.com",
                Email = "customer2@example.com",
                EmailConfirmed = true,
                FirstName = "Bob",
                LastName = "Smith",
                Role = UserRole.CUSTOMER // Set the custom Role enum property
            };
            var result = await userManager.CreateAsync(customer2, "Customer@123"); // *** CHANGE THIS PASSWORD IN PRODUCTION ***
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer2, nameof(UserRole.CUSTOMER));
                logger.LogInformation("Customer 2 user seeded successfully.");
            }
            else
            {
                logger.LogError($"Failed to seed Customer 2 user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}
// --- END: Database Seeding Logic ---

app.Run();