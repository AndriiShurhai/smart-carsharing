using Xunit;
using Moq;
using SmartCarSharingApp.UI.ViewModels;
using SmartCarSharing.Core.Services;
using System.Threading.Tasks;

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

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.RegisterUserAsync("Andrii", "test@example.com", "securePass123"), Times.Once);
        }

        [Fact]
        public async Task RegisterCommand_ShouldNotCallService_WhenNameIsEmpty()
        {
            _viewModel.Name = ""; 
            _viewModel.Email = "valid@email.com";
            _viewModel.Password = "12345";

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterCommand_ShouldNotCallService_WhenEmailIsEmpty()
        {
            _viewModel.Name = "User";
            _viewModel.Email = "   ";
            _viewModel.Password = "12345";

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterCommand_ShouldNotCallService_WhenPasswordIsEmpty()
        {
            _viewModel.Name = "User";
            _viewModel.Email = "test@test.com";
            _viewModel.Password = null; 

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}