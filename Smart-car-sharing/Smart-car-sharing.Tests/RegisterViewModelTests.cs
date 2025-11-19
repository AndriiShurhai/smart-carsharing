using Xunit;
using Moq;
using SmartCarSharingApp.UI.ViewModels;
using SmartCarSharing.Core.Services;
using System.Threading.Tasks;
using System;

namespace SmartCarSharingApp.Tests
{
    public class RegisterViewModelTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly RegisterViewModel _viewModel;

        public RegisterViewModelTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _viewModel = new RegisterViewModel(_mockAuthService.Object);
        }

        [Fact]
        public async Task RegisterCommand_ShouldCallAuthService_WhenAllFieldsAreValid()
        {
            _viewModel.Name = "Andrii";
            _viewModel.Email = "test@example.com";
            _viewModel.Password = "securePass123";
            _viewModel.ConfirmPassword = "securePass123"; 

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.RegisterUserAsync("Andrii", "test@example.com", "securePass123"), Times.Once);
        }

        [Fact]
        public void RegisterCommand_CanExecute_ShouldBeFalse_WhenPasswordsDoNotMatch()
        {
            _viewModel.Name = "Test";
            _viewModel.Email = "test@test.com";
            _viewModel.Password = "123456";
            _viewModel.ConfirmPassword = "654321"; 

            bool canExecute = _viewModel.RegisterCommand.CanExecute(null);

            Assert.False(canExecute);
        }

        [Fact]
        public void RegisterCommand_CanExecute_ShouldBeFalse_WhenAnyFieldIsEmpty()
        {
            _viewModel.Name = "";
            _viewModel.Email = "test@test.com";
            _viewModel.Password = "123";
            _viewModel.ConfirmPassword = "123";

            bool canExecute = _viewModel.RegisterCommand.CanExecute(null);

            Assert.False(canExecute);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSetErrorMessage_WhenEmailExists()
        {
            _viewModel.Name = "Test";
            _viewModel.Email = "exist@test.com";
            _viewModel.Password = "123";
            _viewModel.ConfirmPassword = "123";

            _mockAuthService
                .Setup(s => s.RegisterUserAsync(It.IsAny<string>(), "exist@test.com", It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Користувач вже існує"));

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.Equal("Користувач вже існує", _viewModel.ErrorMessage);
        }
    }
}