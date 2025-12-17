using Xunit;
using Moq;
using SmartCarSharingApp.UI.ViewModels;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Core;
using System;
using System.Threading.Tasks;

namespace SmartCarSharingApp.Tests
{
    public class BookingViewModelTests
    {
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly Car _testCar;

        public BookingViewModelTests()
        {
            // Arrange: Setup common mocks
            _mockBookingService = new Mock<IBookingService>();

            _testCar = new Car
            {
                Id = 1,
                Make = "TestMake",
                Model = "Model 3",
                PricePerHour = 100m
            };
        }

        [Fact]
        public void TotalPrice_ShouldUpdate_WhenDatesChange()
        {
            // Arrange
            // Tell the mock to return 500 when CalculatePrice is called
            _mockBookingService
                .Setup(s => s.CalculatePrice(It.IsAny<Car>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(500m);

            var viewModel = new BookingViewModel(_testCar, _mockBookingService.Object);

            // Act: Change the date (this triggers CalculatePrice via partial methods)
            viewModel.EndDate = viewModel.StartDate.AddHours(5);

            // Assert
            Assert.Equal(500m, viewModel.TotalPrice);
            // Verify the service method was actually called
            _mockBookingService.Verify(s => s.CalculatePrice(It.IsAny<Car>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ConfirmCommand_CanExecute_ShouldBeFalse_IfPriceIsZero()
        {
            // Arrange
            // Simulate invalid dates logic (Service returns 0)
            _mockBookingService
                .Setup(s => s.CalculatePrice(It.IsAny<Car>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(0m);

            var viewModel = new BookingViewModel(_testCar, _mockBookingService.Object);

            // Act
            // Trigger update to set TotalPrice to 0
            viewModel.StartDate = DateTime.Now.AddDays(5);
            viewModel.EndDate = DateTime.Now.AddDays(1);

            // Assert
            Assert.Equal(0m, viewModel.TotalPrice);
            Assert.False(viewModel.ConfirmCommand.CanExecute(null));
        }

        [Fact]
        public void ConfirmCommand_CanExecute_ShouldBeTrue_IfPriceIsPositive()
        {
            // Arrange
            _mockBookingService
                .Setup(s => s.CalculatePrice(It.IsAny<Car>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(100m); // Valid price

            var viewModel = new BookingViewModel(_testCar, _mockBookingService.Object);

            // Act
            viewModel.EndDate = DateTime.Now.AddHours(1);

            // Assert
            Assert.True(viewModel.ConfirmCommand.CanExecute(null));
        }

        [Fact]
        public async Task ConfirmCommand_ShouldCallService_WhenExecuted()
        {
            // Arrange
            var viewModel = new BookingViewModel(_testCar, _mockBookingService.Object);

            // Set static AppState for the test context
            AppState.CurrentUser = new User { Id = 10, Name = "Tester", Email = "test@test.com" };

            // Setup successful result from service
            _mockBookingService
                .Setup(s => s.CreateBookingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(BookingResult.Success(new Booking { TotalCost = 100 }));

            // Act
            await viewModel.ConfirmCommand.ExecuteAsync(null);

            // Assert
            // Verify CreateBookingAsync was called EXACTLY once with specific ID parameters
            _mockBookingService.Verify(s => s.CreateBookingAsync(
                10,             // User Id
                _testCar.Id,    // Car Id
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }
    }
}