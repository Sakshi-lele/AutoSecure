using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.Identity; // <--- ADD THIS LINE to reference your new factory
using Microsoft.AspNetCore.Authentication.Cookies; // <--- ADD THIS LINE for ConfigureApplicationCookie options

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>(); // <--- THIS IS THE KEY ADDITION FOR METHOD 1

// Configure the Identity cookie to redirect to your AuthController for AccessDenied
builder.Services.ConfigureApplicationCookie(options =>
{
    // If you ever want to change the login path, set it here.
    // Ensure this matches the route to your AuthController.Login action.
    options.LoginPath = "/Auth/Login";

    // This is the crucial part for the Access Denied redirect:
    options.AccessDeniedPath = "/Auth/AccessDenied";

    // Optional: Cookie expiration and sliding expiration settings
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Example: cookie expires after 60 minutes
    options.SlidingExpiration = true; // Renews cookie expiration if user is active after half the ExpirationTimeSpan
    options.Cookie.HttpOnly = true; // Recommended for security
    options.Cookie.IsEssential = true; // Marks the cookie as essential for the app to function
});


// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // <--- RECOMMENDED: Add this if you use any Identity UI or other Razor Pages

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) // <--- Change to IsDevelopment() for detailed errors during dev
{
    app.UseDeveloperExceptionPage(); // <--- Recommended for development to see detailed errors
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// This is important for Identity UI if you're using it (e.g., /Identity/Account/Login)
app.MapRazorPages(); // <--- ADD THIS LINE

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();