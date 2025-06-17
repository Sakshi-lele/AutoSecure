using Auto_Insurance_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public interface ISupportService
    {
        Task<SupportTicket> CreateTicketAsync(SupportTicket ticket);
        Task<SupportTicket> GetTicketByIdAsync(int id);
        Task<List<SupportTicket>> GetTicketsByUserIdAsync(string userId);
        Task<List<SupportTicket>> GetAllTicketsAsync();
        Task<SupportTicket> UpdateTicketAsync(SupportTicket ticket);
        Task<TicketResponse> AddResponseAsync(TicketResponse response);
        Task<List<SupportTicket>> GetTicketsByPolicyIdAsync(int policyId);
        Task<List<SupportTicket>> SearchTicketsAsync(string searchTerm, TicketStatus? status);
    }
}