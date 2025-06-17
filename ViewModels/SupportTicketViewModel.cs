using Auto_Insurance_Management_System.Models;
using System.Collections.Generic;

namespace Auto_Insurance_Management_System.ViewModels
{
    public class SupportTicketVM
    {
        public SupportTicket Ticket { get; set; }
        public TicketResponse NewResponse { get; set; }
        public List<Policy> UserPolicies { get; set; }
    }
}