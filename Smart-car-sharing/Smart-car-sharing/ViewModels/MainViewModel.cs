using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core; // Додано для класу Car
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data.Services;
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

        private readonly ICarService _carService;

        private BookingViewModel _bookingViewModel;

        private void NavigateToBooking(Car car)
        {
            _bookingViewModel = new BookingViewModel(car);

            // Логіка кнопок у вікні бронювання
            _bookingViewModel.RequestCancel += () =>
            {
                // При скасуванні повертаємось до деталей авто
                NavigateToCarDetails(car);
            };

            _bookingViewModel.RequestConfirm += () =>
            {
                // При успіху можна повернутись на Dashboard
                System.Windows.MessageBox.Show("Бронювання успішно створено! (Демо)", "Успіх");
                CurrentViewModel = _dashboardViewModel;
            };

            CurrentViewModel = _bookingViewModel;
        }

        public MainViewModel(
            IAuthenticationService authService,
            ICarService carService)
        {
            _authService = authService;
            _carService = carService;

            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            _dashboardViewModel = new DashboardViewModel(_carService);

            _dashboardViewModel.RequestNavigateToDetails += NavigateToCarDetails;

            CurrentViewModel = _dashboardViewModel;
        }

        private void NavigateToCarDetails(Car car)
        {
            _carDetailsViewModel = new CarDetailsViewModel(car);

            // Навігація назад
            _carDetailsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;

            // --- ДОДАНО: Навігація до бронювання ---
            _carDetailsViewModel.RequestNavigateToBooking += NavigateToBooking;

            CurrentViewModel = _carDetailsViewModel;
        }
    }
}
