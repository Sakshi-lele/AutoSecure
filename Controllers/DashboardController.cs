using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using Auto_Insurance_Management_System.Models; // Often not directly needed in DashboardController if only rendering UI

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize] // Ensures only authenticated users can access any action in this controller
    public class DashboardController : Controller
    {
        // This is the default entry point if someone just navigates to /Dashboard or /Dashboard/Index
        public IActionResult Index()
        {
            // Determine current user's role
            string userRole = "CUSTOMER"; // Default to CUSTOMER

            if (User.IsInRole("ADMIN"))
            {
                userRole = "ADMIN";
            }
            else if (User.IsInRole("AGENT"))
            {
                userRole = "AGENT";
            }
            // If none of the above, it remains CUSTOMER (as per the default or if a user has no specific role).

            ViewBag.UserRole = userRole; // Standardized ViewBag key for the view
            ViewBag.UserName = User.Identity.Name;

            return View(); // Renders Views/Dashboard/Index.cshtml
        }

        // Admin-specific dashboard action
        [Authorize(Roles = "ADMIN")]
        public IActionResult AdminDashboard()
        {
            ViewBag.UserRole = "ADMIN"; // Explicitly set role for the view
            ViewBag.UserName = User.Identity.Name;
            return View("Index"); // Render the shared Views/Dashboard/Index.cshtml
        }

        // Agent-specific dashboard action
        [Authorize(Roles = "AGENT")]
        public IActionResult AgentDashboard()
        {
            ViewBag.UserRole = "AGENT"; // Explicitly set role for the view
            ViewBag.UserName = User.Identity.Name;
            return View("Index"); // Render the shared Views/Dashboard/Index.cshtml
        }

        // Customer-specific dashboard action
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult CustomerDashboard()
        {
            ViewBag.UserRole = "CUSTOMER"; // Explicitly set role for the view
            ViewBag.UserName = User.Identity.Name;
            return View("Index"); // Render the shared Views/Dashboard/Index.cshtml
        }
    }
}