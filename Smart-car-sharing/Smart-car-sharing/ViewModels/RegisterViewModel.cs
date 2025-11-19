using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core.Services;
using System.Threading.Tasks;
using System.Windows; 

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        public RegisterViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            await _authService.RegisterUserAsync(Name, Email, Password);
        }
    }
}