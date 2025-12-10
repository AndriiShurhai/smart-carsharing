using System.Windows;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharing.Core.Services;
using SmartCarSharingApp.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace SmartCarSharingApp.UI
{
    public partial class App : Application
    {
        // We keep a reference to the factory so we can pass loggers to other services
        public static ILoggerFactory? LoggerFactory { get; private set; }

        private AppDbContext _context;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // 2. Create the wrapper that allows us to inject ILogger<T>
            LoggerFactory = new SerilogLoggerFactory();

            var appLoger = LoggerFactory.CreateLogger<App>();
            appLoger.LogInformation("------------------");
            appLoger.LogInformation("Smart Carsharing app Started");
            appLoger.LogInformation("------------------");


            // 3. Setup Database
            _context = new AppDbContext();
            // Ensure DB is created
            _context.Database.EnsureCreated();

            // 4. Setup Services
            IAuthenticationService authService = new AuthenticationService(_context);

            var mainViewModel = new MainViewModel(authService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            _context.Dispose();
            base.OnExit(e);
        }
    }
}