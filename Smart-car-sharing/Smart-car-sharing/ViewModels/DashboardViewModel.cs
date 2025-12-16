using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using System;
using System.Collections.ObjectModel;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // Колекція, до якої прив'язується інтерфейс
        [ObservableProperty]
        private ObservableCollection<Car> _cars;

        [ObservableProperty]
        private string _searchText;

        // Подія, яку слухає MainViewModel для перемикання екрану
        public Action<Car> RequestNavigateToDetails { get; set; }

        public DashboardViewModel()
        {
            Cars = new ObservableCollection<Car>();
            LoadMockData();
        }

        private void LoadMockData()
        {
            // Тестові дані для перевірки відображення сітки
            Cars.Add(new Car { Make = "Tesla", Model = "Model 3", Year = 2022, PricePerHour = 45, Location = "Київ, Центр" });
            Cars.Add(new Car { Make = "Toyota", Model = "Camry", Year = 2020, PricePerHour = 25, Location = "Київ, Аеропорт" });
            Cars.Add(new Car { Make = "BMW", Model = "X5", Year = 2023, PricePerHour = 60, Location = "Львів, Вокзал" });
            Cars.Add(new Car { Make = "Ford", Model = "Focus", Year = 2019, PricePerHour = 20, Location = "Одеса" });
            Cars.Add(new Car { Make = "Audi", Model = "Q7", Year = 2021, PricePerHour = 55, Location = "Київ, Поділ" });
            Cars.Add(new Car { Make = "Nissan", Model = "Leaf", Year = 2018, PricePerHour = 18, Location = "Харків" });
        }

        // Ця команда викликається, коли натискають "Деталі" на картці
        [RelayCommand]
        private void ViewDetails(Car car)
        {
            if (car != null)
            {
                RequestNavigateToDetails?.Invoke(car);
            }
        }
    }
}