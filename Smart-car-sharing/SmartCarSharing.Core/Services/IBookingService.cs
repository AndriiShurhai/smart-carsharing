using System;
using System.Threading.Tasks;
using SmartCarSharing.Core;

namespace SmartCarSharing.Core.Services
{
    public interface IBookingService
    {
        Task<BookingResult> CreateBookingAsync(int userId, int carId, DateTime start, DateTime end);

        // ЗМІНЕНО: Було (int carId, int days) -> Стало (..., double hours)
        Task<decimal> CalculatePriceAsync(int carId, double hours);

        decimal CalculatePrice(Car car, DateTime start, DateTime end);
    }

    public class BookingResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Booking? Booking { get; set; }

        public static BookingResult Success(Booking booking) => new() { IsSuccess = true, Booking = booking };
        public static BookingResult Failure(string message) => new() { IsSuccess = false, Message = message };
    }
}