using System.Windows;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharing.Core.Services;
using SmartCarSharingApp.UI.ViewModels;

namespace SmartCarSharingApp.UI
{
    public partial class App : Application
    {
        private AppDbContext _context;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Setup Database
            _context = new AppDbContext();
            // Ensure DB is created
            _context.Database.EnsureCreated();

            // 2. Setup Services
            IAuthenticationService authService = new AuthenticationService(_context);

            // 3. Setup Main ViewModel
            var mainViewModel = new MainViewModel(authService);

            // 4. Show Window
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _context.Dispose();
            base.OnExit(e);
        }
    }
}