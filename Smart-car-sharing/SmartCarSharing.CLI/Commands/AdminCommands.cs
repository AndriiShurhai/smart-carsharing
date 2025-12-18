using Microsoft.EntityFrameworkCore;
using SmartCarSharing.CLI.Architecture;
using SmartCarSharing.Data;
using System.Linq;

namespace SmartCarSharing.CLI.Commands
{
    public class ListUsersCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextWriter _writer;

        public ListUsersCommand(AppDbContext context, TextWriter writer)
        {
            _context = context;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            var users = _context.Users.ToList();
            _writer.WriteLine($"--- Registered Users ({users.Count}) ---");
            foreach (var user in users)
            {
                _writer.WriteLine($"ID: {user.Id} | Name: {user.Name} | License: {user.DriverLicenseNumber}");
            }
            return CommandResult.CONTINUE;
        }

        public string Name() => "list-users";
    }

    public class ListCarsCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextWriter _writer;

        public ListCarsCommand(AppDbContext context, TextWriter writer)
        {
            _context = context;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            var cars = _context.Cars.ToList();
            _writer.WriteLine($"--- Fleet Status ({cars.Count}) ---");
            foreach (var car in cars)
            {
                _writer.WriteLine($"[{car.Id}] {car.Make} {car.Model} ({car.Year}) - ${car.PricePerHour}/hr - {car.Location}");
            }
            return CommandResult.CONTINUE;
        }

        public string Name() => "list-cars";
    }

    public class StatsCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextWriter _writer;

        public StatsCommand(AppDbContext context, TextWriter writer)
        {
            _context = context;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            var bookingCount = _context.Bookings.Count();
            var revenue = _context.Bookings
                .Select(b => b.TotalCost)
                .ToList()
                .Sum();

            _writer.WriteLine("--- System Statistics ---");
            _writer.WriteLine($"Total Bookings: {bookingCount}");
            _writer.WriteLine($"Total Revenue:  ${revenue}");
            return CommandResult.CONTINUE;
        }

        public string Name() => "stats";
    }
}