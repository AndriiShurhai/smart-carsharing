using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCarSharing.Core;
using SmartCarSharing.Core.DTOs;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
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
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var mockLogger = new Mock<ILogger<BookingService>>();

            _bookingService = new BookingService(_context, mockLogger.Object);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_ShouldReturnCorrectBookings_SortedByDateDescending()
        {
            var user1 = new User { Id = 1, Name = "User1", Email = "u1@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL123" };
            var user2 = new User { Id = 2, Name = "User2", Email = "u2@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL456" };

            var car1 = new Car { Id = 1, Make = "Tesla", Model = "Model 3", PricePerHour = 50, Year = 2023, Location = "Kyiv" };
            var car2 = new Car { Id = 2, Make = "BMW", Model = "X5", PricePerHour = 80, Year = 2022, Location = "Lviv" };

            _context.Users.AddRange(user1, user2);
            _context.Cars.AddRange(car1, car2);

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

            _context.Bookings.AddRange(booking1, booking2, booking3);
            await _context.SaveChangesAsync();

            var results = await _bookingService.GetBookingsByUserIdAsync(user1.Id);

            Assert.Equal(2, results.Count);

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
            var user = new User { Id = 1, Name = "User1", Email = "u1@test.com", HashedPassword = "hash", DriverLicenseNumber = "DL1" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var results = await _bookingService.GetBookingsByUserIdAsync(user.Id);

            Assert.Empty(results);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}