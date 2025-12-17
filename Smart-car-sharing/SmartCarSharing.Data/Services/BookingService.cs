using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Core;
using SmartCarSharing.Core.DTOs;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public decimal CalculatePrice(Car car, DateTime start, DateTime end)
        {
            if (end <= start) return 0;
            var duration = end - start;
            var hours = Math.Max(1, Math.Ceiling(duration.TotalHours));
            return (decimal)hours * car.PricePerHour;
        }

        public async Task<decimal> CalculatePriceAsync(int carId, double hours)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) throw new ArgumentException("Car not found");

            decimal billableHours = (decimal)hours;
            return car.PricePerHour * billableHours;
        }

        public decimal CalculatePrice(Car car, DateTime start, DateTime end)
            return car.PricePerHour * (decimal)hours;
        }

        public async Task<BookingResult> CreateBookingAsync(int userId, int carId, DateTime start, DateTime end)
        {
            _logger.LogInformation($"Validating booking for User {userId}, Car {carId}...");

            var duration = end - start;
            // Округляємо до повної години вгору (мінімум 1 година)
            var hours = Math.Max(1, Math.Ceiling(duration.TotalHours));
            // --- 1. ПЕРЕВІРКИ КОРИСТУВАЧА ---
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return BookingResult.Failure("Користувача не знайдено.");

            // Rule 3 & 8: Наявність та формат прав
            if (string.IsNullOrWhiteSpace(user.DriverLicenseNumber))
            {
                return BookingResult.Failure("У профілі відсутнє водійське посвідчення.");
            }

            // Простий Regex: Мінімум 5 символів, букви та цифри (можна адаптувати під українські права)
            if (!Regex.IsMatch(user.DriverLicenseNumber, @"^[A-Z0-9]{5,15}$", RegexOptions.IgnoreCase))
            {
                return BookingResult.Failure("Невірний формат водійського посвідчення (має бути 5-15 літер/цифр).");
            }

            // --- 2. ПЕРЕВІРКИ ЧАСУ ---
            var now = DateTime.Now;

            // Rule 1: Дата початку не в минулому (даємо 2 хвилини люфту на затримку мережі/кліків)
            if (start < now.AddMinutes(-2))
            {
                return BookingResult.Failure("Не можна бронювати на минулий час.");
            }

            // Rule 1.1: Дата закінчення пізніше дати початку
            if (end <= start)
            {
                return BookingResult.Failure("Дата закінчення має бути пізніше дати початку.");
            }

            // Rule 5: Мінімальна тривалість 1 година
            if ((end - start).TotalHours < 1.0)
            {
                return BookingResult.Failure("Мінімальний час оренди — 1 година.");
            }

            // Rule 6: Максимальне бронювання наперед (6 місяців)
            if (start > now.AddMonths(6))
            {
                return BookingResult.Failure("Бронювання доступне лише на найближчі 6 місяців.");
            }

            // --- 3. ПЕРЕВІРКИ АВТОМОБІЛЯ ---
            var car = await _context.Cars.FindAsync(carId);
            if (car == null) return BookingResult.Failure("Автомобіль не знайдено.");

            // Rule 4: Перевірка на перетин (Overlapping)
            // Логіка: Новий інтервал (Start, End) перетинається з існуючим (b.Start, b.End),
            // якщо (Start < b.End) І (End > b.Start).
            bool isOccupied = await _context.Bookings
                .AnyAsync(b => b.CarId == carId &&
                               start < b.EndTime &&
                               end > b.StartTime);

            if (isOccupied)
            {
                return BookingResult.Failure("Автомобіль вже заброньовано на цей період.");
            }

            // --- ЗБЕРЕЖЕННЯ ---
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
                _logger.LogInformation("Booking created successfully.");
                return BookingResult.Success(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database Error");
                return BookingResult.Failure("Помилка бази даних.");
            }
        }
    }
}