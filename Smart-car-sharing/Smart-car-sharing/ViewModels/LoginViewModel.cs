using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        public Action RequestNavigateToRegister { get; set; }

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _email;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isBusy;

        private bool CanLogin()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = await _authService.LoginUserAsync(Email, Password);

                if (user == null)
                {
                    ErrorMessage = "Невірний email або пароль.";
                    return;
                }

                AppState.CurrentUser = user;

                MessageBox.Show($"Вітаємо, {user.Name}!", "Успішний вхід");

                // Here we would navigate to the Dashboard/Home view usually
            }
            catch (Exception ex)
            {
                ErrorMessage = "Помилка під час входу: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            RequestNavigateToRegister?.Invoke();
        }
    }
}