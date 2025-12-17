using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using System;
using System.Threading.Tasks;

namespace SmartCarSharing.Data.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(AppDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ЗМІНЕНО: Приймаємо години (double), а не дні
        public async Task<decimal> CalculatePriceAsync(int carId, double hours)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) throw new ArgumentException("Car not found");

            // Логіка: Ціна за годину * кількість годин
            // Math.Ceiling округлює вгору (наприклад, 1.2 години = 2 години оплати),
            // але можна прибрати Ceiling, якщо хочете точну оплату за хвилини.
            // Для каршерингу часто округлюють до хвилини, але поки лишимо години:

            decimal billableHours = (decimal)hours;

            return car.PricePerHour * billableHours;
        }

        // Цей метод вже був правильним, але переконаємось
        public decimal CalculatePrice(Car car, DateTime start, DateTime end)
        {
            if (end <= start) return 0;

            var duration = end - start;

            // TotalHours повертає дробове число (наприклад 1.5 для півтори години)
            // Використовуємо Math.Ceiling, щоб округлити до повної години в більшу сторону
            // (1 година 10 хв = оплата за 2 години)
            var hours = Math.Max(1, Math.Ceiling(duration.TotalHours));

            return (decimal)hours * car.PricePerHour;
        }

        public async Task<BookingResult> CreateBookingAsync(int userId, int carId, DateTime start, DateTime end)
        {
            _logger.LogInformation($"Attempting to book Car {carId} for User {userId} from {start} to {end}");

            if (start >= end)
            {
                return BookingResult.Failure("Дата закінчення має бути пізніше дати початку.");
            }
            if (start < DateTime.Now.AddMinutes(-5)) // Даємо 5 хв "люфту"
            {
                return BookingResult.Failure("Не можна бронювати на минулий час.");
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return BookingResult.Failure("Автомобіль не знайдено.");
            }

            // Перевірка перетинів (залишається без змін)
            bool isOccupied = await _context.Bookings
                .AnyAsync(b => b.CarId == carId &&
                               start < b.EndTime &&
                               end > b.StartTime);

            if (isOccupied)
            {
                return BookingResult.Failure("Авто зайняте на цей час.");
            }

            decimal totalCost = CalculatePrice(car, start, end);

            var booking = new Booking
            {
                UserId = userId,
                CarId = carId,
                StartTime = start,
                EndTime = end,
                TotalCost = totalCost
            };

            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return BookingResult.Success(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB Error");
                return BookingResult.Failure("Помилка збереження.");
            }
        }
    }
}