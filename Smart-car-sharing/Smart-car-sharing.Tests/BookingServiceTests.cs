using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCarSharing.Core;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SmartCarSharingApp.Tests
{
    public class BookingServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            // 1. Налаштовуємо базу даних в пам'яті (In-Memory SQLite)
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            // 2. Мокаємо логер
            var mockLogger = new Mock<ILogger<BookingService>>();

            // 3. Створюємо сервіс
            _bookingService = new BookingService(_context, mockLogger.Object);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_ShouldReturnBookingsWithCarDetails_OrderedByDate()
        {
            // Arrange (Підготовка даних)
            var user1Id = 1;
            var user2Id = 2;

            // Додаємо машини
            var car1 = new Car { Id = 1, Make = "Tesla", Model = "Model S", PricePerHour = 100, Year = 2022, Location = "Kyiv" };
            var car2 = new Car { Id = 2, Make = "BMW", Model = "X5", PricePerHour = 150, Year = 2023, Location = "Lviv" };
            _context.Cars.AddRange(car1, car2);

            // Додаємо користувачів (для цілісності FK, хоча SQLite може пропустити)
            _context.Users.Add(new User { Id = user1Id, Name = "User1", Email = "u1@test.com", HashedPassword = "hash" });
            _context.Users.Add(new User { Id = user2Id, Name = "User2", Email = "u2@test.com", HashedPassword = "hash" });

            // Додаємо бронювання
            // Бронювання 1 (Старіше)
            var booking1 = new Booking
            {
                UserId = user1Id,
                CarId = car1.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(2),
                TotalCost = 2400
            };

            // Бронювання 2 (Новіше - має бути першим у списку)
            var booking2 = new Booking
            {
                UserId = user1Id,
                CarId = car2.Id,
                StartTime = DateTime.Now.AddDays(5), // Пізніша дата
                EndTime = DateTime.Now.AddDays(6),
                TotalCost = 3600
            };

            // Бронювання іншого юзера (не повинно потрапити у вибірку)
            var bookingOther = new Booking
            {
                UserId = user2Id,
                CarId = car1.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                TotalCost = 200
            };

            _context.Bookings.AddRange(booking1, booking2, bookingOther);
            await _context.SaveChangesAsync();

            // Act (Дія)
            var result = await _bookingService.GetBookingsByUserIdAsync(user1Id);

            // Assert (Перевірка)

            // 1. Має знайти тільки 2 бронювання для User1
            Assert.Equal(2, result.Count);

            // 2. Перевірка сортування (спочатку новіші за датою старту)
            Assert.Equal(booking2.TotalCost, result[0].TotalCost); // Booking 2 (BMW) новіше
            Assert.Equal(booking1.TotalCost, result[1].TotalCost); // Booking 1 (Tesla) старіше

            // 3. Перевірка JOIN (чи підтягнулись дані про машину)
            Assert.Equal("BMW", result[0].CarMake);
            Assert.Equal("X5", result[0].CarModel);

            Assert.Equal("Tesla", result[1].CarMake);
            Assert.Equal("Model S", result[1].CarModel);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}