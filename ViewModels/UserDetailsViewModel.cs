using Auto_Insurance_Management_System.Models;
namespace Auto_Insurance_Management_System.ViewModels
{
    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<PaymentDetailsViewModel> DuePayments { get; set; } = new List<PaymentDetailsViewModel>();
    }
}