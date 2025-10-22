using Xunit;
using SmartCarSharingApp.Core;

namespace SmartCarSharingApp.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void Add_ShouldReturnCorrectSum()
        {
            // Arrange: Set up the test
            var calculator = new Calculator();
            int expected = 5;

            // Act: Run the method you're testing
            int actual = calculator.Add(2, 3);

            // Assert: Verify the result is correct
            Assert.Equal(expected, actual);
        }
    }
}