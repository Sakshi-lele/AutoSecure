using Auto_Insurance_Management_System.Models;
using Auto_Insurance_Management_System.ViewModels;

namespace Auto_Insurance_Management_System.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginViewModel model);
        Task<bool> RegisterUserAsync(RegisterViewModel model);
        Task LogoutAsync();
        Task<User> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(string userId, UserRole role);
        Task<bool> DeactivateUserAsync(string userId);
    }
}