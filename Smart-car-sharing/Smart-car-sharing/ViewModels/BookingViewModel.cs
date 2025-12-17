using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services; // ! Важливо
using System;
using System.Threading.Tasks;
using System.Windows; // Для MessageBox

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class BookingViewModel : ObservableObject
    {
        private readonly Car _car;
        private readonly IBookingService _bookingService; // ! Додано залежність

        public Action? RequestCancel { get; set; }
        public Action? RequestConfirm { get; set; }

        // ! Конструктор змінено: додано bookingService
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

        [ObservableProperty] // ! Додано, щоб блокувати кнопку, поки йде запит
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private bool _isBusy;

        public string TotalPriceString => $"{TotalPrice:C}";

        partial void OnStartDateChanged(DateTime value) => CalculatePrice();
        partial void OnEndDateChanged(DateTime value) => CalculatePrice();

        private void CalculatePrice()
        {
            if (EndDate < StartDate)
            {
                TotalPrice = 0;
                return;
            }
            var duration = EndDate - StartDate;
            var hours = Math.Max(1, Math.Ceiling(duration.TotalHours));
            TotalPrice = (decimal)hours * _car.PricePerHour;
        }

        // ! Перевірка для команди: чи можна натиснути кнопку?
        private bool CanConfirm()
        {
            return !IsBusy && TotalPrice > 0;
        }

        // ! Оновлена команда Confirm (тепер асинхронна)
        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private async Task ConfirmAsync()
        {
            // Перевірка авторизації
            if (AppState.CurrentUser == null)
            {
                MessageBox.Show("Будь ласка, увійдіть у систему, щоб забронювати авто.", "Помилка");
                return;
            }

            IsBusy = true; // Блокуємо кнопку

            try
            {
                // Створюємо об'єкт бронювання
                var booking = new Booking
                {
                    CarId = _car.Id,
                    UserId = AppState.CurrentUser.Id,
                    StartTime = StartDate,
                    EndTime = EndDate,
                    TotalCost = TotalPrice
                };

                // Викликаємо сервіс (зберігаємо в БД)
                await _bookingService.CreateBookingAsync(booking);

                MessageBox.Show("Успіх! Автомобіль заброньовано.", "Бронювання");

                // Повертаємось на головну
                RequestConfirm?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при бронюванні: {ex.Message}", "Помилка");
            }
            finally
            {
                IsBusy = false; // Розблокуємо кнопку
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestCancel?.Invoke();
        }
    }
}