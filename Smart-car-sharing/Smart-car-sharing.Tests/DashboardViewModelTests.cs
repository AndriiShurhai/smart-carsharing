using Xunit;
using Moq;
using SmartCarSharingApp.UI.ViewModels;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmartCarSharingApp.Tests
{
    public class DashboardViewModelTests
    {
        private readonly Mock<ICarService> _mockCarService;

        public DashboardViewModelTests()
        {
            _mockCarService = new Mock<ICarService>();
        }

        [Fact]
        public async Task Constructor_ShouldLoadCars_FromService()
        {
            var expectedCars = new List<Car>
            {
                new Car { Id = 1, Make = "Tesla", Model = "Model 3" },
                new Car { Id = 2, Make = "BMW", Model = "X5" }
            };

            _mockCarService.Setup(s => s.GetAllCarsAsync())
                           .ReturnsAsync(expectedCars);

            var viewModel = new DashboardViewModel(_mockCarService.Object);
            await Task.Delay(50);

            Assert.NotNull(viewModel.Cars);
            Assert.Equal(2, viewModel.Cars.Count);
            Assert.Equal("Tesla", viewModel.Cars[0].Make);
            _mockCarService.Verify(s => s.GetAllCarsAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchCommand_ShouldCall_GetFilteredCarsAsync()
        {
            _mockCarService.Setup(s => s.GetAllCarsAsync()).ReturnsAsync(new List<Car>());
            var viewModel = new DashboardViewModel(_mockCarService.Object);

            var searchResults = new List<Car> { new Car { Make = "Audi" } };
            _mockCarService.Setup(s => s.GetFilteredCarsAsync("Audi"))
                           .ReturnsAsync(searchResults);

            viewModel.SearchText = "Audi";

            await viewModel.SearchCommand.ExecuteAsync(null);

            Assert.Single(viewModel.Cars);
            Assert.Equal("Audi", viewModel.Cars.First().Make);
            _mockCarService.Verify(s => s.GetFilteredCarsAsync("Audi"), Times.Once);
        }

        [Fact]
        public void OpenDetailsCommand_ShouldTrigger_RequestNavigateToDetails()
        {
            var viewModel = new DashboardViewModel(_mockCarService.Object);
            var testCar = new Car { Id = 1, Make = "Test" };
            Car? navigatedCar = null;

            viewModel.RequestNavigateToDetails = (car) => navigatedCar = car;

            viewModel.OpenDetailsCommand.Execute(testCar);

            Assert.Equal(testCar, navigatedCar);
        }

        [Fact]
        public async Task LoadAllCarsAsync_ShouldToggle_IsBusy()
        {
            var tcs = new TaskCompletionSource<List<Car>>();
            _mockCarService.Setup(s => s.GetAllCarsAsync()).Returns(tcs.Task);

            var viewModel = new DashboardViewModel(_mockCarService.Object);

            Assert.True(viewModel.IsBusy);

            tcs.SetResult(new List<Car>());
            await Task.Delay(10);

            Assert.False(viewModel.IsBusy); 
        }
    }
}