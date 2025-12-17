using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ICarService _carService;

        // ====== PROPERTIES ======

        [ObservableProperty]
        private ObservableCollection<Car> cars = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        // ====== NAVIGATION ======
        public Action<Car>? RequestNavigateToDetails { get; set; }

        // ====== CONSTRUCTOR ======
        public DashboardViewModel(ICarService carService)
        {
            _carService = carService;
            _ = LoadAllCarsAsync();
        }

        // ====== COMMANDS ======

        [RelayCommand]
        private async Task LoadAllCarsAsync()
        {
            IsBusy = true;

            try
            {
                var result = await _carService.GetAllCarsAsync();
                Cars = new ObservableCollection<Car>(result);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSearch))]
        private async Task SearchAsync()
        {
            IsBusy = true;

            try
            {
                var result = await _carService.GetFilteredCarsAsync(SearchText);
                Cars = new ObservableCollection<Car>(result);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSearch()
        {
            return !IsBusy;
        }

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