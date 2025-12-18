using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using System;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class CarDetailsViewModel : ObservableObject
    {
        private readonly Car _car;

        public Action? RequestGoBack { get; set; }

        // --- ДОДАНО: Порожній конструктор для XAML дизайнера ---
        public CarDetailsViewModel()
        {
            // Створюємо тестові дані, щоб у Visual Studio було видно красиву картинку
            _car = new Car
            {
                Make = "Test Make",
                Model = "Test Model",
                Year = 2024,
                PricePerHour = 99,
                Location = "Test Location"
            };
        }
        public Action<Car>? RequestNavigateToBooking { get; set; }

        [RelayCommand]
        private void BookNow()
        {
            RequestNavigateToBooking?.Invoke(_car);
        }
        public CarDetailsViewModel(Car car)
        {
            _car = car ?? throw new ArgumentNullException(nameof(car));
        }

        public string FullName => $"{_car.Make} {_car.Model} ({_car.Year})";

        // Форматування валюти
        public string PriceFormatted => $"{_car.PricePerHour:C} / год";

        public string Location => _car.Location;
        public string Make => _car.Make;
        public string Model => _car.Model;
        public int Year => _car.Year;
        public decimal Price => _car.PricePerHour;

        [RelayCommand]
        private void GoBack()
        {
            RequestGoBack?.Invoke();
        }
    }
}