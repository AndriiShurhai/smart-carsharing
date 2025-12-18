using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services; // Added
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace SmartCarSharingApp.UI.ViewModels
{
    public class BookingItem
    {
        public int Id { get; set; }
        public string CarName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalCost { get; set; }

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
        private readonly IBookingService _bookingService; // Added Service

        [ObservableProperty]
        private ObservableCollection<BookingItem> _bookings = new();

        [ObservableProperty]
        private bool _isBusy;

        public Action? RequestGoBack { get; set; }

        // Inject IBookingService into the constructor
        public MyBookingsViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Method to load real data from DB
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var currentUser = AppState.CurrentUser;
                if (currentUser == null)
                {
                    // Should not happen if logic flows correctly, but safety first
                    return; 
                }

                var bookingDtos = await _bookingService.GetBookingsByUserIdAsync(currentUser.Id);

                Bookings.Clear();

                foreach (var dto in bookingDtos)
                {
                    Bookings.Add(new BookingItem
                    {
                        Id = dto.Id,
                        CarName = $"{dto.CarMake} {dto.CarModel}", // Combine Make + Model
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        TotalCost = dto.TotalCost
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            RequestGoBack?.Invoke();
        }
    }
}