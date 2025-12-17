using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core.Services;

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;
        public Action? RequestNavigateToLogin { get; set; }

        public RegisterViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string name = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string email = string.Empty;

        // НОВЕ ПОЛЕ
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string driverLicense = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string password = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private bool isBusy;

        private bool CanRegister()
        {
            bool isFilled = !string.IsNullOrWhiteSpace(Name) &&
                            !string.IsNullOrWhiteSpace(Email) &&
                            !string.IsNullOrWhiteSpace(DriverLicense) && // Перевірка
                            !string.IsNullOrWhiteSpace(Password) &&
                            !string.IsNullOrWhiteSpace(ConfirmPassword);

            bool isEmailValid = Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            // Валідація прав на стороні UI (візуальна)
            bool isLicenseValid = Regex.IsMatch(DriverLicense, @"^[A-Z0-9]{5,15}$", RegexOptions.IgnoreCase);

            bool passwordsMatch = Password == ConfirmPassword;

            return isFilled && isEmailValid && isLicenseValid && passwordsMatch && !IsBusy;
        }

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Паролі не співпадають!";
                    return;
                }

                // Передаємо права у сервіс
                await _authService.RegisterUserAsync(Name, Email, Password, DriverLicense);

                RequestNavigateToLogin?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            RequestNavigateToLogin?.Invoke();
        }
    }
}