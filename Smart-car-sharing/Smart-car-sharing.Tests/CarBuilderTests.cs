using Xunit;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Builders;

namespace SmartCarSharingApp.Tests
{
    public class CarBuilderTests
    {
        [Fact]
        public void Build_ShouldCreateCar_WithCorrectProperties()
        {
            // Arrange
            var builder = new CarBuilder();

            // Act
            Car result = builder
                .WithModel("Tesla", "Model S")
                .WithYear(2023)
                .WithPrice(100m)
                .WithLocation("Kyiv")
                .Build();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Tesla", result.Make);
            Assert.Equal("Model S", result.Model);
            Assert.Equal(2023, result.Year);
            Assert.Equal(100m, result.PricePerHour);
            Assert.Equal("Kyiv", result.Location);
        }

        [Fact]
        public void Build_ShouldReset_AfterCreatingCar()
        {
            // Arrange
            var builder = new CarBuilder();

            // Act
            // Build first car
            builder.WithModel("BMW", "X5").Build();

            // Build second car (without setting properties)
            Car emptyCar = builder.Build();

            // Assert
            // The builder should have reset to defaults (Id=0, null strings)
            Assert.Equal(0, emptyCar.Id);
            Assert.Null(emptyCar.Make);
        }
    }
}