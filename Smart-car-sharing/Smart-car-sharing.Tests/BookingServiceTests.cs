using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharing.Core;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace SmartCarSharingApp.Tests
{
    public class BookingServiceTests
    {
        // Helper method to create a fresh In-Memory DB Context for every test
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name ensures clean DB per test
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void CalculatePrice_ShouldRoundUpHours_AndCalculateCorrectly()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var car = new Car { PricePerHour = 100 }; // 100 USD per hour
            var start = DateTime.Now;
            var end = start.AddMinutes(90); // 1 hour 30 minutes

            // Act
            // Logic: 1.5 hours should round UP to 2 hours. 2 * 100 = 200.
            decimal price = service.CalculatePrice(car, start, end);

            // Assert
            Assert.Equal(200, price);
        }

        [Fact]
        public async Task CreateBooking_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            // Seed Data
            var user = new User { Id = 1, Name = "Test User", Email = "test@test.com", HashedPassword = "hash", DriverLicenseNumber = "ABC12345" };
            var car = new Car { Id = 1, Make = "Tesla", Model = "Model 3", PricePerHour = 50, Location = "Center" };
            context.Users.Add(user);
            context.Cars.Add(car);
            await context.SaveChangesAsync();

            var start = DateTime.Now.AddDays(1); // Future date
            var end = start.AddHours(2);

            // Act
            var result = await service.CreateBookingAsync(1, 1, start, end);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Booking);
            Assert.Equal(100, result.Booking.TotalCost); // 50 * 2 hours
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenDatesOverlap()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var user = new User { Id = 1, Name = "User", DriverLicenseNumber = "ABC12345", Email = "a", HashedPassword = "p" };
            var car = new Car { Id = 1, PricePerHour = 50, Make = "Test", Model = "Test", Location = "Test" };

            // Existing Booking: Tomorrow 12:00 - 14:00
            var existingBooking = new Booking
            {
                UserId = 1,
                CarId = 1,
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(12),
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(14)
            };

            context.Users.Add(user);
            context.Cars.Add(car);
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            // Act
            // New Request: Tomorrow 13:00 - 15:00 (Overlaps in the middle)
            var start = DateTime.Now.AddDays(1).Date.AddHours(13);
            var end = DateTime.Now.AddDays(1).Date.AddHours(15);

            var result = await service.CreateBookingAsync(1, 1, start, end);

            // Assert
            Assert.False(result.IsSuccess);
            // Виправлено відповідно до попередньої ітерації (слово "заброньовано")
            Assert.Contains("заброньовано", result.Message.ToLower());
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenDateIsInPast()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var user = new User { Id = 1, DriverLicenseNumber = "ABC12345", Name = "U", Email = "E", HashedPassword = "P" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var start = DateTime.Now.AddHours(-5); // 5 hours ago
            var end = DateTime.Now.AddHours(-3);

            var result = await service.CreateBookingAsync(1, 1, start, end);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("минулий", result.Message.ToLower());
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenDurationIsTooShort()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var user = new User { Id = 1, DriverLicenseNumber = "ABC12345", Name = "U", Email = "E", HashedPassword = "P" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var start = DateTime.Now.AddDays(1);
            var end = start.AddMinutes(30); // Only 30 minutes

            var result = await service.CreateBookingAsync(1, 1, start, end);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("1 година", result.Message);
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenLicenseIsInvalid()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            // User with INVALID license (too short)
            var user = new User { Id = 1, Name = "Bad Driver", DriverLicenseNumber = "123", Email = "e", HashedPassword = "p" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var start = DateTime.Now.AddDays(1);
            var end = start.AddHours(2);

            var result = await service.CreateBookingAsync(1, 1, start, end);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("посвідчення", result.Message.ToLower());
        }

        // --- НОВІ ТЕСТИ (з вашого коду) ---

        [Fact]
        public async Task GetBookingsByUserIdAsync_ShouldReturnCorrectBookings_SortedByDateDescending()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var user1 = new User { Id = 1, Name = "User1", Email = "u1@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL123" };
            var user2 = new User { Id = 2, Name = "User2", Email = "u2@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL456" };

            var car1 = new Car { Id = 1, Make = "Tesla", Model = "Model 3", PricePerHour = 50, Year = 2023, Location = "Kyiv" };
            var car2 = new Car { Id = 2, Make = "BMW", Model = "X5", PricePerHour = 80, Year = 2022, Location = "Lviv" };

            context.Users.AddRange(user1, user2);
            context.Cars.AddRange(car1, car2);

            var booking1 = new Booking
            {
                UserId = user1.Id,
                CarId = car1.Id,
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(2),
                TotalCost = 100
            };

            var booking2 = new Booking
            {
                UserId = user1.Id,
                CarId = car2.Id,
                StartTime = DateTime.Now.AddDays(5),
                EndTime = DateTime.Now.AddDays(6),
                TotalCost = 200
            };

            var booking3 = new Booking
            {
                UserId = user2.Id,
                CarId = car1.Id,
                StartTime = DateTime.Now.AddDays(3),
                EndTime = DateTime.Now.AddDays(4),
                TotalCost = 150
            };

            context.Bookings.AddRange(booking1, booking2, booking3);
            await context.SaveChangesAsync();

            // Act
            var results = await service.GetBookingsByUserIdAsync(user1.Id);

            // Assert
            Assert.Equal(2, results.Count);

            // Перевіряємо сортування (спочатку новіші, якщо метод це підтримує, інакше просто наявність)
            // У вашому тесті ви очікували booking2 (день 5) першим, а booking1 (день 1) другим.
            // Це означає сортування Descending.
            Assert.Equal(booking2.StartTime, results[0].StartTime);
            Assert.Equal(booking1.StartTime, results[1].StartTime);

            Assert.Equal("BMW", results[0].CarMake);
            Assert.Equal("X5", results[0].CarModel);
            Assert.Equal("Tesla", results[1].CarMake);
            Assert.Equal("Model 3", results[1].CarModel);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_ShouldReturnEmpty_WhenNoBookingsExist()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = new Mock<ILogger<BookingService>>();
            var service = new BookingService(context, logger.Object);

            var user = new User { Id = 1, Name = "User1", Email = "u1@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL1" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var results = await service.GetBookingsByUserIdAsync(user.Id);

            // Assert
            Assert.Empty(results);
        }
    }
}