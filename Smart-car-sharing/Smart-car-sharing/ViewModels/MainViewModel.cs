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
        private readonly MyBookingsViewModel _myBookingsViewModel; 

        [ObservableProperty]
        private object _currentViewModel;

        private readonly ICarService _carService;

        public MainViewModel(
            IAuthenticationService authService,
            ICarService carService)
        {
            _authService = authService;
            _carService = carService;

            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            _dashboardViewModel = new DashboardViewModel(_carService);
            _myBookingsViewModel = new MyBookingsViewModel();

            _dashboardViewModel.RequestNavigateToDetails += NavigateToCarDetails;

            _dashboardViewModel.RequestNavigateToMyBookings += () =>
            {
                CurrentViewModel = _myBookingsViewModel;
            };

            _myBookingsViewModel.RequestGoBack += () =>
            {
                CurrentViewModel = _dashboardViewModel;
            };

            _loginViewModel.RequestNavigateToRegister += () => CurrentViewModel = _registerViewModel;
            _registerViewModel.RequestNavigateToLogin += () => CurrentViewModel = _loginViewModel;

            CurrentViewModel = _dashboardViewModel;
        }


        private void NavigateToCarDetails(Car car)
        {
            _carDetailsViewModel = new CarDetailsViewModel(car);

            _carDetailsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;

            CurrentViewModel = _carDetailsViewModel;
        }
    }
}