using CommunityToolkit.Mvvm.ComponentModel;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data.Services; // Якщо потрібно для namespace
using System;
using System.Windows;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // --- Сервіси ---
        private readonly IAuthenticationService _authService;
        private readonly ICarService _carService;
        private readonly IBookingService _bookingService; // Новий сервіс

        // --- Дочірні ViewModels ---
        private readonly LoginViewModel _loginViewModel;
        private readonly RegisterViewModel _registerViewModel;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly MyBookingsViewModel _myBookingsViewModel;

        // Ці створюються динамічно, бо залежать від конкретного авто
        private CarDetailsViewModel _carDetailsViewModel;
        private BookingViewModel _bookingViewModel;

        // --- Поточна ViewModel (для відображення в ContentControl) ---
        [ObservableProperty]
        private object _currentViewModel;

        public MainViewModel(
            IAuthenticationService authService,
            ICarService carService,
            IBookingService bookingService) // Отримуємо сервіс через конструктор
        {
            _authService = authService;
            _carService = carService;
            _bookingService = bookingService;

            // Ініціалізація постійних VM
            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            _dashboardViewModel = new DashboardViewModel(_carService);
            _myBookingsViewModel = new MyBookingsViewModel();

            // --- Налаштування навігації ---

            // 1. Логін -> Реєстрація
            _loginViewModel.RequestNavigateToRegister += () => CurrentViewModel = _registerViewModel;

            // 2. Реєстрація -> Логін
            _registerViewModel.RequestNavigateToLogin += () => CurrentViewModel = _loginViewModel;

            // *Тимчасово*: Після успішного логіну можна перемикати на Dashboard (логіку додамо пізніше або вручну)
            // Поки що стартуємо з Dashboard для зручності розробки, 
            // але в реальному застосунку стартували б з _loginViewModel.

            // 3. Dashboard -> Деталі авто
            _dashboardViewModel.RequestNavigateToDetails += NavigateToCarDetails;

            // 4. Dashboard -> Мої бронювання
            _dashboardViewModel.RequestNavigateToMyBookings += () =>
            {
                // Тут можна було б оновити дані в _myBookingsViewModel перед показом
                CurrentViewModel = _myBookingsViewModel;
            };

            // 5. Мої бронювання -> Назад (на Dashboard)
            _myBookingsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;

            // Встановлюємо стартовий екран
            CurrentViewModel = _dashboardViewModel;
        }

        private void NavigateToCarDetails(Car car)
        {
            _carDetailsViewModel = new CarDetailsViewModel(car);

            // Навігація назад до списку
            _carDetailsViewModel.RequestGoBack += () => CurrentViewModel = _dashboardViewModel;

            // Навігація до бронювання
            _carDetailsViewModel.RequestNavigateToBooking += NavigateToBooking;
            CurrentViewModel = _carDetailsViewModel;
        }

        private void NavigateToBooking(Car car)
        {
            // Створюємо VM для бронювання, передаючи авто ТА сервіс
            _bookingViewModel = new BookingViewModel(car, _bookingService);

            // Логіка кнопки "Скасувати"
            _bookingViewModel.RequestCancel += () =>
            {
                // Повертаємось до деталей цього авто
                NavigateToCarDetails(car);
            };

            // Логіка успішного підтвердження
            _bookingViewModel.RequestConfirm += () =>
            {
                // Повертаємось на головну (Dashboard)
                CurrentViewModel = _dashboardViewModel;
            };

            CurrentViewModel = _bookingViewModel;
        }
    }
}