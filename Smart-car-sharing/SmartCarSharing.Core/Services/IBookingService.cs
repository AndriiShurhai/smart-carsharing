using SmartCarSharing.Core;
using System.Threading.Tasks;

namespace SmartCarSharing.Core.Services
{
    public interface IBookingService
    {
        Task CreateBookingAsync(Booking booking);
    }
}