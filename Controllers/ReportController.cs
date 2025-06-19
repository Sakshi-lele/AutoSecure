using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int dateRange = 30, string policyStatus = "all", string claimStatus = "all")
        {
            try
            {
                // Calculate date range
                DateTime startDate = dateRange > 0 ? DateTime.UtcNow.AddDays(-dateRange) : DateTime.MinValue;

                // Fetch metrics
                var dashboardData = new
                {
                    // Policies data
                    totalPolicies = await _context.Policies.CountAsync(),
                    activePolicies = await _context.Policies.CountAsync(p => p.PolicyStatus == "ACTIVE"),
                    renewedPolicies = await _context.Policies.CountAsync(p => p.PolicyStatus == "RENEWED"),
                    policiesTrend = 8.5, // Sample trend value

                    // Claims data
                    totalClaims = await _context.Claims.CountAsync(),
                    avgClaimAmount = await _context.Claims.AverageAsync(c => (decimal?)c.ClaimAmountRequested) ?? 0,
                    rejectedClaimsPercent = (int)(
                        (await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Rejected || c.Status == ClaimStatus.Declined) /
                        (double)await _context.Claims.CountAsync()) * 100),
                    claimsTrend = -2.3, // Sample trend value

                    // Revenue data
                    totalRevenue = await _context.Payments
                        .Where(p => p.PaymentDate >= DateTime.UtcNow.AddDays(-30) && p.Status == PaymentStatus.Completed)
                        .SumAsync(p => p.Amount),
                    cardPaymentPercent = 68, // Sample value
                    upiPaymentPercent = 24,   // Sample value
                    revenueTrend = 7.8,       // Sample trend value

                    // Support tickets data
                    totalTickets = await _context.SupportTickets.CountAsync(),
                    openTickets = await _context.SupportTickets.CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress),
                    billingTicketPercent = (int)(
                        (await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.Billing) /
                        (double)await _context.SupportTickets.CountAsync()) * 100),
                    ticketsTrend = 4.2, // Sample trend value

                    // Charts data
                    claimsStatus = new Dictionary<string, int>
                    {
                        { "Submitted", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Submitted) },
                        { "Verified", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Verified) },
                        { "UnderReview", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.UnderReview) },
                        { "Approved", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Approved) },
                        { "Settled", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Settled) },
                        { "Rejected", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Rejected) },
                        { "Declined", await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Declined) }
                    },

                    policyCoverageTypes = await _context.Policies
                        .GroupBy(p => p.CoverageType)
                        .Select(g => new { Type = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Type, x => x.Count),

                    monthlyClaims = new
                    {
                        currentYear = await GetMonthlyClaims(DateTime.UtcNow.Year),
                        previousYear = await GetMonthlyClaims(DateTime.UtcNow.Year - 1)
                    },

                    userRoles = new Dictionary<string, int>
                    {
                        { "CUSTOMER", await _context.Users.CountAsync(u => u.Role == UserRole.CUSTOMER) },
                        { "AGENT", await _context.Users.CountAsync(u => u.Role == UserRole.AGENT) },
                        { "ADMIN", await _context.Users.CountAsync(u => u.Role == UserRole.ADMIN) }
                    },

                    paymentStatus = new Dictionary<string, int>
                    {
                        { "Completed", await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Completed) },
                        { "Pending", await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending) },
                        { "Failed", await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Failed) },
                        { "Declined", await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Declined) },
                        { "Refunded", await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Refunded) }
                    },

                    supportTicketTypes = new Dictionary<string, int>
                    {
                        { "Billing", await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.Billing) },
                        { "Claim", await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.Claim) },
                        { "Policy Change", await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.PolicyChange) },
                        { "Cancellation", await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.Cancellation) },
                        { "Other", await _context.SupportTickets.CountAsync(t => t.QueryType == QueryType.Other) }
                    },

                    recentClaims = await _context.Claims
                        .Include(c => c.Policy)
                        .ThenInclude(p => p.User)
                        .OrderByDescending(c => c.DateOfSubmission)
                        .Take(5)
                        .Select(c => new
                        {
                            claimId = c.ClaimId,
                            policyNumber = c.Policy.PolicyNumber,
                            customerName = $"{c.Policy.User.FirstName} {c.Policy.User.LastName}",
                            claimType = c.ClaimType,
                            amount = c.ClaimAmountRequested,
                            status = c.Status.ToString(),
                            dateSubmitted = c.DateOfSubmission
                        }).ToListAsync()
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving dashboard data: {ex.Message}");
            }
        }

        private async Task<List<int>> GetMonthlyClaims(int year)
        {
            var claims = await _context.Claims
                .Where(c => c.DateOfSubmission.Year == year)
                .GroupBy(c => c.DateOfSubmission.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var monthlyData = new List<int>();
            for (int month = 1; month <= 12; month++)
            {
                var monthData = claims.FirstOrDefault(c => c.Month == month);
                monthlyData.Add(monthData?.Count ?? 0);
            }

            return monthlyData;
        }
    }
}
