using System.Threading.Tasks;
using SmartCarSharing.Core;

namespace SmartCarSharing.Core.Services
{
    public interface IAuthenticationService
    {
        Task RegisterUserAsync(string name, string email, string password, string driverLicense);
        Task<User?> LoginUserAsync(string email, string password);
    }
}