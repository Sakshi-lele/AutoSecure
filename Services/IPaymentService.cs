// Services/IPaymentService.cs
using System.Threading.Tasks;

namespace Auto_Insurance_Management_System.Services
{
    public interface IPaymentService
    {
        Task<int> GetPendingPaymentsCountAsync();
        Task<int> GetCompletedPaymentsCountAsync();
    }
}