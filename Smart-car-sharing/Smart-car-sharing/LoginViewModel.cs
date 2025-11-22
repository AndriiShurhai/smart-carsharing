using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;

namespace SmartCarSharingApp.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async (_) => await LoginAsync(), (_) => CanLogin());
        }

        // -----------------------
        // Properties
        // -----------------------
        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        // -----------------------
        // Commands
        // -----------------------
        public ICommand LoginCommand { get; }

        private bool CanLogin()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        // -----------------------
        // Login Logic
        // -----------------------
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

                // Глобальна сесія користувача
                AppState.CurrentUser = user;

                // Відкриваємо головне вікно
                var mainWindow = new Smart_car_sharing.MainWindow();

                // Закриваємо LoginWindow
                Application.Current.MainWindow?.Close();

                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
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

        // -----------------------
        // INotifyPropertyChanged
        // -----------------------
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // -----------------------
    // RelayCommand
    // -----------------------
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // -----------------------
    // Глобальний стан користувача
    // -----------------------
    public static class AppState
    {
        public static User CurrentUser { get; set; }
    }
}
