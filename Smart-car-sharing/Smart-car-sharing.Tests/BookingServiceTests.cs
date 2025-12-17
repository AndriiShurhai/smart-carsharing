using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharing.Core;
using System;
using System.Threading.Tasks;

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
            var context = GetInMemoryContext();
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
            Assert.Contains("автомобіль вже заброньовано на цей період", result.Message.ToLower()); // Checks for "occupied" in Ukrainian
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
            Assert.Contains("минулий", result.Message.ToLower()); // Checks for "past" in Ukrainian
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
    }
}