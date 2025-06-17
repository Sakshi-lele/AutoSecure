using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.Services;
using Auto_Insurance_Management_System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ISupportService _supportService;
        private readonly ApplicationDbContext _context; // Now recognized

        public SupportController(
            ISupportService supportService, 
            ApplicationDbContext context) // Now recognized
        {
            _supportService = supportService;
            _context = context;
        }

        // GET: Support
        public async Task<IActionResult> Index(string searchTerm, TicketStatus? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRole.ADMIN.ToString());
            var isAgent = User.IsInRole(UserRole.AGENT.ToString());

            List<SupportTicket> tickets;

            if (isAdmin || isAgent)
            {
                tickets = await _supportService.SearchTicketsAsync(searchTerm, status);
            }
            else
            {
                tickets = await _supportService.GetTicketsByUserIdAsync(userId);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    tickets = tickets.Where(t => 
                        t.Description.Contains(searchTerm) ||
                        t.Policy.PolicyNumber.Contains(searchTerm))
                    .ToList();
                }
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            return View(tickets);
        }

        // GET: Support/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _supportService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRole.ADMIN.ToString());
            var isAgent = User.IsInRole(UserRole.AGENT.ToString());

            if (!isAdmin && !isAgent && ticket.UserId != currentUserId)
            {
                return Forbid();
            }

            ViewBag.NewResponse = new TicketResponse { TicketId = id };
            return View(ticket);
        }

        // GET: Support/Create
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userPolicies = _context.Policies
                .Where(p => p.UserId == userId)
                .ToList();

            ViewBag.UserPolicies = userPolicies;
            return View(new SupportTicket { UserId = userId });
        }

        // POST: Support/Create
        // SupportController.cs

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("PolicyId,QueryType,Description")] SupportTicket ticket)
{
    // Get user ID once at the start
    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    try
    {
        Console.WriteLine($"Starting ticket creation for user: {User.Identity?.Name}");
        
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User identification failed. Please re-login.";
            return RedirectToAction("Create");
        }
        ticket.UserId = userId;

        // Remove specific model state entries
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("Policy");
        ModelState.Remove("Responses");

        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.Open;

        Console.WriteLine($"Ticket data: PolicyId={ticket.PolicyId}, QueryType={ticket.QueryType}, Description={ticket.Description?.Length} chars");

        if (ModelState.IsValid)
        {
            Console.WriteLine("ModelState is valid. Creating ticket...");
            await _supportService.CreateTicketAsync(ticket);
            Console.WriteLine($"Ticket created with ID: {ticket.TicketId}");
            
            TempData["SuccessMessage"] = "Support ticket created successfully!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            Console.WriteLine("ModelState errors:");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating ticket: {ex}");
        TempData["ErrorMessage"] = "An error occurred while creating the ticket. Please try again.";
    }

    // Reload policies using the same userId variable
    ViewBag.UserPolicies = await _context.Policies
        .Where(p => p.UserId == userId)
        .ToListAsync();
    
    Console.WriteLine("Returning to Create view with errors");
    return View(ticket);
}
        // POST: Support/AddResponse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse([Bind("TicketId,Content,IsInternalNote")] TicketResponse response)
        {
            response.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            response.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                await _supportService.AddResponseAsync(response);
                return RedirectToAction("Details", new { id = response.TicketId });
            }

            // If invalid, reload the details view with errors
            var ticket = await _supportService.GetTicketByIdAsync(response.TicketId);
            ViewBag.NewResponse = response;
            return View("Details", ticket);
        }

        // POST: Support/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "ADMIN,AGENT")]
        public async Task<IActionResult> UpdateStatus(int id, TicketStatus status)
        {
            var ticket = await _supportService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _supportService.UpdateTicketAsync(ticket);

            return RedirectToAction("Details", new { id });
        }
    }
}