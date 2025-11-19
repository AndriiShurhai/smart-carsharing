using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarSharing.Core.Services
{
    public interface IAuthenticationService
    {
        Task RegisterUserAsync(string name, string email, string password);
        Task<User> LoginUserAsync(string email, string password);
    }
}
