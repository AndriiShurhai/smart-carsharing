using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data.Services; // Якщо потрібно для namespace
using System;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService;

        // Child ViewModels
        private readonly LoginViewModel _loginViewModel;
        private readonly RegisterViewModel _registerViewModel;
        private readonly DashboardViewModel _dashboardViewModel;
        private CarDetailsViewModel _carDetailsViewModel;
        private BookingViewModel _bookingViewModel;
        private CarDetailsViewModel _carDetailsViewModel; // Створюється динамічно
        private readonly MyBookingsViewModel _myBookingsViewModel; 

        [ObservableProperty]
        private object _currentViewModel;

        public MainViewModel(
            IAuthenticationService authService,
            ICarService carService,
            IBookingService bookingService)
        {
            _authService = authService;
            _carService = carService;
            _bookingService = bookingService;

            // Ініціалізація VM
            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            _dashboardViewModel = new DashboardViewModel(_carService);
            _myBookingsViewModel = new MyBookingsViewModel();

            // --- НАЛАШТУВАННЯ НАВІГАЦІЇ ---

            // 1. Login -> Register
            _loginViewModel.RequestNavigateToRegister += () => CurrentViewModel = _registerViewModel;

            // 2. Register -> Login
            _registerViewModel.RequestNavigateToLogin += () => CurrentViewModel = _loginViewModel;

            // 3. ДОДАНО: Login -> Dashboard (Успішний вхід)
            _loginViewModel.RequestNavigateToDashboard += () => CurrentViewModel = _dashboardViewModel;

            // 4. Dashboard -> CarDetails
            _dashboardViewModel.RequestNavigateToDetails += NavigateToCarDetails;

            // --- СТАРТОВИЙ ЕКРАН ---

            // Змінено з _dashboardViewModel на _loginViewModel
            CurrentViewModel = _loginViewModel;
        }

        private void NavigateToCarDetails(Car car)
        {
            _carDetailsViewModel = new CarDetailsViewModel(car);
            _carDetailsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;
            _carDetailsViewModel.RequestNavigateToBooking += NavigateToBooking;
            CurrentViewModel = _carDetailsViewModel;
        }

        private void NavigateToBooking(Car car)
        {
            _bookingViewModel = new BookingViewModel(car, _bookingService);

            _bookingViewModel.RequestCancel += () =>
            {
                NavigateToCarDetails(car);
            };

            _bookingViewModel.RequestConfirm += () =>
            {
                CurrentViewModel = _dashboardViewModel;
            };

            CurrentViewModel = _bookingViewModel;
        }
    }
}