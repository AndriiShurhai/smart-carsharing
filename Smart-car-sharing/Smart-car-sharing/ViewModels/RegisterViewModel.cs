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
                            !string.IsNullOrWhiteSpace(Password) &&
                            !string.IsNullOrWhiteSpace(ConfirmPassword);

            bool isEmailValid = Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            bool passwordsMatch = Password == ConfirmPassword;

            return isFilled && isEmailValid && passwordsMatch && !IsBusy;
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

                await _authService.RegisterUserAsync(Name, Email, Password);

                RequestNavigateToLogin?.Invoke();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                ErrorMessage = "Сталася невідома помилка. Спробуйте пізніше.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}