using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Core;
using SmartCarSharing.Core.DTOs;
using SmartCarSharing.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // --- Метод для отримання бронювань користувача (JOIN з таблицею Cars) ---
        public async Task<List<BookingDto>> GetBookingsByUserIdAsync(int userId)
        {
            _logger.LogInformation("Fetching bookings for user {UserId}", userId);

            // Використовуємо JOIN, щоб отримати назву авто разом з даними про бронювання
            var query = from b in _context.Bookings
                        join c in _context.Cars on b.CarId equals c.Id
                        where b.UserId == userId
                        orderby b.StartTime descending // Сортуємо: найновіші зверху
                        select new BookingDto
                        {
                            Id = b.Id,
                            CarMake = c.Make,
                            CarModel = c.Model,
                            StartTime = b.StartTime,
                            EndTime = b.EndTime,
                            TotalCost = b.TotalCost
                        };

            var result = await query.ToListAsync();
            _logger.LogInformation("Found {Count} bookings for user {UserId}", result.Count, userId);

            return result;
        }

        // --- Інші методи сервісу ---

        public async Task<decimal> CalculatePriceAsync(int carId, double hours)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) throw new ArgumentException("Car not found");

            decimal billableHours = (decimal)hours;
            return car.PricePerHour * billableHours;
        }

        public decimal CalculatePrice(Car car, DateTime start, DateTime end)
        {
            if (end <= start) return 0;

            var duration = end - start;
            // Округляємо до повної години вгору (мінімум 1 година)
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
            if (start < DateTime.Now.AddMinutes(-5))
            {
                return BookingResult.Failure("Не можна бронювати на минулий час.");
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return BookingResult.Failure("Автомобіль не знайдено.");
            }

            // Перевірка на перетин дат
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