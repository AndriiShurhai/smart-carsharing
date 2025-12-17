using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Builders;
using SmartCarSharing.Core.Services;
using SmartCarSharing.Data;
using SmartCarSharing.Data.Services;
using SmartCarSharingApp.UI.ViewModels;

namespace SmartCarSharingApp.UI
{
    public partial class App : Application
    {
        public static ILoggerFactory? LoggerFactory { get; private set; }
        private AppDbContext _context;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Налаштування Serilog (Логування у файл)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Створення фабрики логерів для ін'єкції у сервіси
            LoggerFactory = new SerilogLoggerFactory();

            var appLogger = LoggerFactory.CreateLogger<App>();
            appLogger.LogInformation("------------------");
            appLogger.LogInformation("Smart Carsharing App Started");
            appLogger.LogInformation("------------------");

            // 2. Налаштування Бази Даних (SQLite)
            _context = new AppDbContext();
            _context.Database.EnsureCreated();

            // 3. Заповнення початковими даними (Seeding), якщо БД порожня
            if (!_context.Cars.Any())
            {
                appLogger.LogInformation("Database is empty. Seeding with 20 cars...");
                SeedDatabase();
                appLogger.LogInformation("Successfully seeded 20 cars");
            }

            // 4. Ініціалізація сервісів (Dependency Injection "вручну")

            // Сервіс аутентифікації
            IAuthenticationService authService = new AuthenticationService(_context);

            // Сервіс автомобілів (потребує логер)
            ICarService carService = new CarService(
                _context,
                LoggerFactory.CreateLogger<CarService>()
            );

            // Сервіс бронювання (потребує логер)
            IBookingService bookingService = new BookingService(
                _context,
                LoggerFactory.CreateLogger<BookingService>()
            );

            // 5. Створення головної ViewModel з усіма сервісами
            var mainViewModel = new MainViewModel(authService, carService, bookingService);

            // 6. Створення та показ головного вікна
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        // Метод для генерації тестових авто
        private void SeedDatabase()
        {
            var builder = new CarBuilder();
            var carsToSeed = new List<Car>();

            var locations = new[] { "Smila Center", "Smila Airport", "Smila Port", "Smila Railway" };
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            _context?.Dispose();
            base.OnExit(e);
        }
    }
}