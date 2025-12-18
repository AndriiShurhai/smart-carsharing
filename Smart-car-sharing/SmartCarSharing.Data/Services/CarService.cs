using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCarSharing.Core;
using SmartCarSharing.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCarSharing.Data.Services
{
    public class CarService : ICarService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarService> _logger;

        public CarService(AppDbContext context, ILogger<CarService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Car>> GetAllCarsAsync()
        {
            _logger.LogInformation("Fetching all cars...");

            var cars = await _context.Cars
                .Where(c => c.Status == "Available")
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .ToListAsync();

            _logger.LogInformation("Found {Count} cars", cars.Count);

            return cars;
        }

        public async Task<List<Car>> GetFilteredCarsAsync(string search)
        {
            _logger.LogInformation("Fetching cars with filter: {Search}", search);

            if (string.IsNullOrWhiteSpace(search))
            {
                _logger.LogInformation("Empty search string, returning all cars");
                return await GetAllCarsAsync();
            }

            search = search.ToLower();

            var cars = await _context.Cars
                .Where(c => c.Status == "Available")
                .Where(c =>
                    c.Make.ToLower().Contains(search) ||
                    c.Model.ToLower().Contains(search) ||
                    c.Location.ToLower().Contains(search) ||
                    c.Year.ToString().Contains(search))
                .OrderBy(c => c.Make)
                .ToListAsync();

            _logger.LogInformation("Found {Count} cars after filtering", cars.Count);

            return cars;
        }
    }
}
