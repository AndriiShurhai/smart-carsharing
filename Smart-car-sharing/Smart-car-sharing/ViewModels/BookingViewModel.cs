using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using System;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class BookingViewModel : ObservableObject
    {
        private readonly Car _car;

        // Події для навігації
        public Action? RequestCancel { get; set; }
        public Action? RequestConfirm { get; set; }

        public BookingViewModel(Car car)
        {
            _car = car ?? throw new ArgumentNullException(nameof(car));

            // Встановлюємо дати за замовчуванням (сьогодні та завтра)
            _startDate = DateTime.Now;
            _endDate = DateTime.Now.AddDays(1);
            CalculatePrice();
        }

        // --- Властивості ---

        public string CarName => $"{_car.Make} {_car.Model}";
        public decimal PricePerHour => _car.PricePerHour;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPriceString))] // Оновлювати ціну при зміні дати
        private DateTime _startDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPriceString))]
        private DateTime _endDate;

        [ObservableProperty]
        private decimal _totalPrice;

        public string TotalPriceString => $"{TotalPrice:C}";

        // --- Логіка ---

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
            // Мінімальний час - 1 година, навіть якщо вибрали менше
            var hours = Math.Max(1, Math.Ceiling(duration.TotalHours));

            TotalPrice = (decimal)hours * _car.PricePerHour;
        }

        // --- Команди ---

        [RelayCommand]
        private void Confirm()
        {
            // Тут буде логіка збереження в БД (в наступних задачах)
            // Поки що просто повідомляємо MainViewModel, що бронювання підтверджено
            RequestConfirm?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestCancel?.Invoke();
        }
    }
}
