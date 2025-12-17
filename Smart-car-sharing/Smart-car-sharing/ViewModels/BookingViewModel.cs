using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class BookingViewModel : ObservableObject
    {
        private readonly Car _car;
        private readonly IBookingService _bookingService;

        public Action? RequestCancel { get; set; }
        public Action? RequestConfirm { get; set; }

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
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))] // Updated: Notify command when price changes
        private decimal _totalPrice;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private bool _isBusy;

        public string TotalPriceString => $"{TotalPrice:C}";

        partial void OnStartDateChanged(DateTime value) => CalculatePrice();
        partial void OnEndDateChanged(DateTime value) => CalculatePrice();

        private void CalculatePrice()
        {
            // Delegates logic to the service
            TotalPrice = _bookingService.CalculatePrice(_car, StartDate, EndDate);
        }

        // Updated: Button is disabled if Price is 0 (invalid dates) or app is busy
        private bool CanConfirm()
        {
            return !IsBusy && TotalPrice > 0;
        }

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