using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services; // Додано
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class BookingViewModel : ObservableObject
    {
        private readonly Car _car;
        private readonly IBookingService _bookingService; // Додано залежність

        public Action? RequestCancel { get; set; }
        public Action? RequestConfirm { get; set; }

        // Конструктор тепер приймає сервіс
        public BookingViewModel(Car car, IBookingService bookingService)
        {
            _car = car ?? throw new ArgumentNullException(nameof(car));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));

            _startDate = DateTime.Now;
            _endDate = DateTime.Now.AddDays(1);
            CalculatePrice();
        }

        public string CarName => $"{_car.Make} {_car.Model}";
        public decimal PricePerHour => _car.PricePerHour;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPriceString))]
        private DateTime _startDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPriceString))]
        private DateTime _endDate;

        [ObservableProperty]
        private decimal _totalPrice;

        [ObservableProperty] // Для блокування кнопки під час запиту
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private bool _isBusy;

        public string TotalPriceString => $"{TotalPrice:C}";

        partial void OnStartDateChanged(DateTime value) => CalculatePrice();
        partial void OnEndDateChanged(DateTime value) => CalculatePrice();

        private void CalculatePrice()
        {
            // Використовуємо синхронну логіку для швидкого відображення в UI,
            // але логіка така сама, як в сервісі.
            TotalPrice = _bookingService.CalculatePrice(_car, StartDate, EndDate);
        }

        private bool CanConfirm() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private async Task ConfirmAsync()
        {
            if (AppState.CurrentUser == null)
            {
                MessageBox.Show("Помилка: Користувач не авторизований.", "Помилка");
                return;
            }

            IsBusy = true;

            try
            {
                // Виклик сервісу
                var result = await _bookingService.CreateBookingAsync(
                    AppState.CurrentUser.Id,
                    _car.Id,
                    StartDate,
                    EndDate);

                if (result.IsSuccess)
                {
                    MessageBox.Show($"Бронювання успішне! Вартість: {result.Booking.TotalCost:C}", "Успіх");
                    RequestConfirm?.Invoke();
                }
                else
                {
                    MessageBox.Show(result.Message, "Помилка бронювання");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка: {ex.Message}", "Помилка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestCancel?.Invoke();
        }
    }
}