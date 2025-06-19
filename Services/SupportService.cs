using Auto_Insurance_Management_System.Data;
using Auto_Insurance_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public class SupportService : ISupportService
    {
        private readonly ApplicationDbContext _context;

        public SupportService(ApplicationDbContext context)
        {
            _context = context;
        }

       public async Task<SupportTicket> CreateTicketAsync(SupportTicket ticket)
{
    try
    {
        Console.WriteLine($"Adding ticket to context: {ticket.Description}");
        _context.SupportTickets.Add(ticket);
        
        Console.WriteLine("Saving changes to database...");
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"Ticket saved with ID: {ticket.TicketId}");
        return ticket;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving ticket: {ex}");
        throw;
    }
}

        public async Task<SupportTicket> GetTicketByIdAsync(int id)
        {
            return await _context.SupportTickets
                .Include(t => t.Policy)
                .Include(t => t.User)
                .Include(t => t.Responses)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

        public async Task<List<SupportTicket>> GetTicketsByUserIdAsync(string userId)
        {
            return await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Policy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SupportTicket>> GetAllTicketsAsync()
        {
            return await _context.SupportTickets
                .Include(t => t.Policy)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket> UpdateTicketAsync(SupportTicket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ticket;
        }

        // public async Task<TicketResponse> AddResponseAsync(TicketResponse response)
        // {
        //     _context.TicketResponses.Add(response);
        //     await _context.SaveChangesAsync();
            
        //     // Update ticket's updated timestamp
        //     var ticket = await _context.SupportTickets.FindAsync(response.TicketId);
        //     if (ticket != null)
        //     {
        //         ticket.UpdatedAt = DateTime.UtcNow;
        //         await _context.SaveChangesAsync();
        //     }
            
        //     return response;
        // }

        public async Task<List<SupportTicket>> GetTicketsByPolicyIdAsync(int policyId)
        {
            return await _context.SupportTickets
                .Where(t => t.PolicyId == policyId)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SupportTicket>> SearchTicketsAsync(string searchTerm, TicketStatus? status)
        {
            var query = _context.SupportTickets
                .Include(t => t.Policy)
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => 
                    t.Description.Contains(searchTerm) ||
                    t.Policy.PolicyNumber.Contains(searchTerm) ||
                    t.User.FirstName.Contains(searchTerm) ||
                    t.User.LastName.Contains(searchTerm));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<TicketResponse> AddResponseAsync(TicketResponse response)
{
    try
    {
        Console.WriteLine($"Adding response to ticket {response.TicketId}");
        _context.TicketResponses.Add(response);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Response added with ID: {response.ResponseId}");

        // Update ticket's updated timestamp
        var ticket = await _context.SupportTickets.FindAsync(response.TicketId);
        if (ticket != null)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        return response;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error adding response: {ex}");
        throw;
    }
}
    }
}