using System.Windows;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharing.Core.Services;
using SmartCarSharingApp.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using SmartCarSharing.Core.Builders;
using SmartCarSharing.Core;

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

            if (!_context.Cars.Any())
            {
                var appLogger = LoggerFactory.CreateLogger<App>();
                appLogger.LogInformation("Database is empty. Seeding with 20 cars...");

                var builder = new CarBuilder();
                var carsToSeed = new List<Car>();

                var locations =  new[] { "Smila Center", "Smila Airport", "Smila Port", "Smila Railway" };

                var models = new[]
                {
                    ("Tesla", "Model 3", 45m),
                    ("Toyota", "Camry", 25m),
                    ("BMW", "X5", 60m),
                    ("Ford", "Focus", 20m),
                    ("Audi", "A6", 55m)
                };

                var random = new Random();

                for (int i = 0; i < 20; i++)
                {
                    var modelInfo = models[random.Next(models.Length)];
                    var location = locations[random.Next(locations.Length)];

                    var car = builder
                        .WithModel(modelInfo.Item1, modelInfo.Item2)
                        .WithYear(random.Next(2018, 2025))
                        .WithPrice(modelInfo.Item3 + random.Next(-5, 5))
                        .WithLocation(location)
                        .Build();
                    carsToSeed.Add(car);
                }
                _context.AddRange(carsToSeed);
                _context.SaveChanges();

                appLogger.LogInformation("Successfully seeded 20 cars");
            }

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