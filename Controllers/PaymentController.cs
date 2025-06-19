// Controllers/PaymentController.cs
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
          public IActionResult Index()
        {
            return View();
        }

        // GET: Payment/DuePayments
        public async Task<IActionResult> DuePayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get active policies for the user
            var policies = await _context.Policies
                .Where(p => p.UserId == userId && p.PolicyStatus == "ACTIVE")
                .ToListAsync();

            // For each policy, determine if there's a due payment
            var duePayments = new List<PaymentDetailsViewModel>();
            
            foreach (var policy in policies)
            {
                // Check if payment is due (simplified logic - in real app, use payment schedule)
                var lastPayment = await _context.Payments
                    .Where(p => p.PolicyId == policy.Id && p.Status == PaymentStatus.Completed)
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefaultAsync();
                
                // If no payment in the last 30 days, it's due
                if (lastPayment == null || lastPayment.PaymentDate < DateTime.UtcNow.AddDays(-30))
                {
                    duePayments.Add(new PaymentDetailsViewModel
                    {
                        PolicyId = policy.Id,
                        PolicyNumber = policy.PolicyNumber,
                        PaymentAmount = policy.PremiumAmount,
                        PaymentDate = DateTime.Today
                    });
                }
            }
            
            return View(duePayments);
        }

        // GET: Payment/MakePayment?policyId=5
        public async Task<IActionResult> MakePayment(int policyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var policy = await _context.Policies
                .FirstOrDefaultAsync(p => p.Id == policyId && p.UserId == userId);
            
            if (policy == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            var model = new PaymentDetailsViewModel
            {
                PolicyId = policyId,
                PolicyNumber = policy.PolicyNumber,
                PaymentAmount = policy.PremiumAmount,
                PaymentDate = DateTime.Today,
                CustomerName = $"{user.FirstName} {user.LastName}"
            };
            
            return View(model);
        }

        // POST: Payment/MakePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(PaymentDetailsViewModel model)
        {
                var payment = new Payment
                {
                    PolicyId = model.PolicyId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Amount = model.PaymentAmount,
                    PaymentDate = model.PaymentDate,
                    DueDate = model.PaymentDate.AddDays(30), // Next due in 30 days
                    PaymentMethod = model.PaymentMethod,
                    CardNumber = model.CardNumber,
                    CardHolderName = model.CardHolderName,
                    ExpiryDate = model.ExpiryDate,
                    CVV = model.CVV,
                    UpiId = model.UpiId,
                    Status = PaymentStatus.Completed, // Assume success for demo
                    TransactionId = Guid.NewGuid().ToString() // Simulated transaction ID
                };
                
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("PaymentSuccess", new { id = payment.PaymentId });
            
            return View(model);
        }

        // GET: Payment/PaymentSuccess/5
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Policy)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
            
            if (payment == null)
            {
                return NotFound();
            }
            
            var model = new PaymentDetailsViewModel
            {
                PaymentId = payment.PaymentId,
                PolicyId = payment.PolicyId,
                PolicyNumber = payment.Policy.PolicyNumber,
                PaymentAmount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                CardNumber = payment.CardNumber,
                CardHolderName = payment.CardHolderName,
                UpiId = payment.UpiId
            };
            
            return View(model);
        }

        // GET: Payment/PaymentHistory
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _context.Payments
                .Include(p => p.Policy)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            return View(payments);
        }
    }
}