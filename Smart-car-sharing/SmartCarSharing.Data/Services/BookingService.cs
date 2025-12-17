using System;
using System.Threading.Tasks;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data;

namespace SmartCarSharing.Data.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateBookingAsync(Booking booking)
        {
            // Додаємо бронювання в БД
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }
    }
}