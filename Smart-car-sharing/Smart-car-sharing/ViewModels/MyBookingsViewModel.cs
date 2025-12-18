using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SmartCarSharingApp.UI.ViewModels
{
    // Допоміжний клас для відображення рядка в таблиці
    public class BookingItem
    {
        public int Id { get; set; }
        public string CarName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalCost { get; set; }

        // Вираховуємо статус на основі часу
        public string Status
        {
            get
            {
                var now = DateTime.Now;
                if (now < StartTime) return "Upcoming";
                if (now >= StartTime && now <= EndTime) return "Active";
                return "Completed";
            }
        }
    }

    public partial class MyBookingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BookingItem> _bookings = new();

        [ObservableProperty]
        private bool _isBusy;

        public Action? RequestGoBack { get; set; }

        public MyBookingsViewModel()
        {
            // У реальному додатку тут ми б викликали сервіс: _bookingService.GetForUser(userId)
            // Зараз завантажимо тестові дані для перевірки UI
            LoadMockData();
        }

        private void LoadMockData()
        {
            IsBusy = true;

            Bookings = new ObservableCollection<BookingItem>
            {
                new BookingItem
                {
                    Id = 1,
                    CarName = "Tesla Model 3",
                    StartTime = DateTime.Now.AddHours(-1), // Почалася годину тому
                    EndTime = DateTime.Now.AddHours(2),    // Закінчиться через 2 години
                    TotalCost = 135
                }, // Це буде "Active"
                
                new BookingItem
                {
                    Id = 2,
                    CarName = "BMW X5",
                    StartTime = DateTime.Now.AddDays(-5),
                    EndTime = DateTime.Now.AddDays(-4),
                    TotalCost = 1200
                }, // Це буде "Completed"

                new BookingItem
                {
                    Id = 3,
                    CarName = "Toyota Camry",
                    StartTime = DateTime.Now.AddDays(1),
                    EndTime = DateTime.Now.AddDays(2),
                    TotalCost = 600
                } // Це буде "Upcoming"
            };

            IsBusy = false;
        }

        [RelayCommand]
        private void GoBack()
        {
            RequestGoBack?.Invoke();
        }
    }
}