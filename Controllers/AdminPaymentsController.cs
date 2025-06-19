// Controllers/AdminPaymentsController.cs
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminPayments
        public async Task<IActionResult> Index(string search = null, string status = null)
        {
            var paymentsQuery = _context.Payments
                .Include(p => p.Policy)
                .Include(p => p.User)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(search))
            {
                paymentsQuery = paymentsQuery.Where(p =>
                    p.Policy.PolicyNumber.Contains(search) ||
                    p.User.FirstName.Contains(search) ||
                    p.User.LastName.Contains(search) ||
                    p.User.Email.Contains(search) ||
                    p.TransactionId.Contains(search));
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<PaymentStatus>(status, out var paymentStatus))
                {
                    paymentsQuery = paymentsQuery.Where(p => p.Status == paymentStatus);
                }
            }
            
            var payments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            ViewBag.Search = search;
            ViewBag.Status = status;
            
            return View(payments);
        }

        // GET: AdminPayments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Policy)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            
            if (payment == null)
            {
                return NotFound();
            }
            
            return View(payment);
        }

        // GET: AdminPayments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Policy)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
            
            if (payment == null)
            {
                return NotFound();
            }
            
            return View(payment);
        }

        // POST: AdminPayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment paymentUpdate)
        {
            if (id != paymentUpdate.PaymentId)
            {
                return NotFound();
            }

            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
            {
                return NotFound();
            }

            // Update only editable fields
            existingPayment.Status = paymentUpdate.Status;
            existingPayment.Amount = paymentUpdate.Amount;
            existingPayment.Notes = paymentUpdate.Notes;

                try
                {
                    _context.Update(existingPayment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(paymentUpdate.PaymentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            
            
            // If model state is invalid, reload related data for the view
            existingPayment.User = await _context.Users.FindAsync(existingPayment.UserId);
            existingPayment.Policy = await _context.Policies.FindAsync(existingPayment.PolicyId);
            return View(existingPayment);
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}