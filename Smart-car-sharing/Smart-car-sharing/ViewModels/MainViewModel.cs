using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core; // Додано для класу Car
using SmartCarSharing.Core.Services;
using System;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        // Child ViewModels
        private readonly LoginViewModel _loginViewModel;
        private readonly RegisterViewModel _registerViewModel;
        private readonly DashboardViewModel _dashboardViewModel;
        private CarDetailsViewModel _carDetailsViewModel; // Створюється динамічно

        [ObservableProperty]
        private object _currentViewModel;

        public MainViewModel(IAuthenticationService authService)
        {
            _authService = authService;

            // Ініціалізація ViewModels
            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            _dashboardViewModel = new DashboardViewModel();

            // --- Налаштування навігації ---

            // 1. Login -> Register
            _loginViewModel.RequestNavigateToRegister += () => CurrentViewModel = _registerViewModel;

            // 2. Register -> Login
            _registerViewModel.RequestNavigateToLogin += () => CurrentViewModel = _loginViewModel;

            // 3. Login -> Dashboard (тимчасова імітація, тут треба додати подію в LoginViewModel)
            // Поки що ми просто стартуємо з Дашборду для тестування

            // 4. Dashboard -> CarDetails
            _dashboardViewModel.RequestNavigateToDetails += NavigateToCarDetails;

            // Запускаємо Дашборд за замовчуванням для перевірки завдання
            CurrentViewModel = _dashboardViewModel;
        }

        private void NavigateToCarDetails(Car car)
        {
            // Створюємо VM деталей для конкретного авто
            _carDetailsViewModel = new CarDetailsViewModel(car);

            // Налаштовуємо кнопку "Назад"
            _carDetailsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;

            CurrentViewModel = _carDetailsViewModel;
        }
    }
}