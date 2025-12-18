using Xunit;
using Moq;
using SmartCarSharing.CLI.Architecture;
using SmartCarSharing.CLI.Commands;
using SmartCarSharing.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;

namespace SmartCarSharing.Tests
{
    public class CliTests
    {
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void Menu_ShouldExecuteCorrectCommand_WhenInputMatches()
        {
            // Arrange
            var input = new StringReader("test-cmd\nexit\n"); 
            var output = new StringWriter();
            var menu = new Menu("Main", input, output);

            var mockCommand = new Mock<ICommand>();
            mockCommand.Setup(c => c.Name()).Returns("test-cmd");
            mockCommand.Setup(c => c.Execute()).Returns(CommandResult.CONTINUE);

            var exitCommand = new Mock<ICommand>();
            exitCommand.Setup(c => c.Name()).Returns("exit");
            exitCommand.Setup(c => c.Execute()).Returns(CommandResult.EXIT);

            menu.Add(mockCommand.Object);
            menu.Add(exitCommand.Object);

            // Act
            menu.Execute();

            // Assert
            mockCommand.Verify(c => c.Execute(), Times.Once); // "test-cmd" was called
            exitCommand.Verify(c => c.Execute(), Times.Once); // "exit" was called
        }

        [Fact]
        public void Menu_ShouldHandleInvalidInput_Gracefully()
        {
            // Arrange
            // Input: invalid-cmd -> exit
            var input = new StringReader("invalid-cmd\nexit\n");
            var output = new StringWriter();
            var menu = new Menu("Main", input, output);

            var exitCommand = new Mock<ICommand>();
            exitCommand.Setup(c => c.Name()).Returns("exit");
            exitCommand.Setup(c => c.Execute()).Returns(CommandResult.EXIT);

            menu.Add(exitCommand.Object);

            // Act
            menu.Execute();

            // Assert
            var outputString = output.ToString();
            Assert.Contains("Command not found", outputString);
        }

        [Fact]
        public void AddVehicleCommand_ShouldParseInput_AndSaveToDb()
        {
            // Arrange
            using var context = GetInMemoryContext();

            // Simulating user input: Make -> Model -> Year -> Price -> Location
            var input = new StringReader("Toyota\nCamry\n2022\n50\nKyiv\n");
            var output = new StringWriter();

            var command = new AddVehicleCommand(context, input, output);

            // Act
            command.Execute();

            // Assert
            Assert.Single(context.Cars);
            var car = context.Cars.First();
            Assert.Equal("Toyota", car.Make);
            Assert.Equal("Camry", car.Model);
            Assert.Equal(50, car.PricePerHour);
        }
    }
}