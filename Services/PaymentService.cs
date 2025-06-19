// Services/PaymentService.cs
using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetPendingPaymentsCountAsync()
        {
            return await _context.Payments
                .CountAsync(p => p.Status == PaymentStatus.Pending);
        }

        public async Task<int> GetCompletedPaymentsCountAsync()
        {
            return await _context.Payments
                .CountAsync(p => p.Status == PaymentStatus.Completed);
        }
    }
}