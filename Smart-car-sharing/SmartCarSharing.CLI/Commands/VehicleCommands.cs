using SmartCarSharing.CLI.Architecture;
using SmartCarSharing.Core;
using SmartCarSharing.Data;
using System;
using System.IO;

namespace SmartCarSharing.CLI.Commands
{
    public class AddVehicleCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextReader _reader;
        private readonly TextWriter _writer;

        public AddVehicleCommand(AppDbContext context, TextReader reader, TextWriter writer)
        {
            _context = context;
            _reader = reader;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            _writer.WriteLine("--- Add New Vehicle ---");

            _writer.Write("Make: ");
            string make = _reader.ReadLine() ?? "Audi";
            make = make.Length == 0 ? "Audi" : make;

            _writer.Write("Model: ");
            string model = _reader.ReadLine() ?? "Ferrari";
            model = model.Length == 0 ? "Ferrari" : model;

            _writer.Write("Year: ");
            if (!int.TryParse(_reader.ReadLine(), out int year) || year < 0) year = DateTime.Now.Year;

            _writer.Write("Price Per Hour: ");
            if (!decimal.TryParse(_reader.ReadLine(), out decimal price) || price < 0) price = 10;

            _writer.Write("Location: ");
            string location = _reader.ReadLine() ?? "Smila";
            location = location.Length == 0 ? "Smila" : location;

            var car = new Car
            {
                Make = make,
                Model = model,
                Year = year,
                PricePerHour = price,
                Location = location
            };

            _context.Cars.Add(car);
            _context.SaveChanges();
            _writer.WriteLine("Vehicle added successfully.");

            return CommandResult.CONTINUE;
        }

        public string Name() => "add-vehicle";
    }

    public class RemoveVehicleCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextReader _reader;
        private readonly TextWriter _writer;

        public RemoveVehicleCommand(AppDbContext context, TextReader reader, TextWriter writer)
        {
            _context = context;
            _reader = reader;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            _writer.Write("Enter Car ID to remove: ");
            if (int.TryParse(_reader.ReadLine(), out int id))
            {
                var car = _context.Cars.Find(id); 
                if (car != null) // null check
                {
                    _context.Cars.Remove(car);
                    _context.SaveChanges();
                    _writer.WriteLine($"Car {id} removed.");
                }
                else
                {
                    _writer.WriteLine("Car not found.");
                }
            }
            else
            {
                _writer.WriteLine("Invalid ID.");
            }
            return CommandResult.CONTINUE;
        }

        public string Name() => "remove-vehicle";
    }
    public class UpdateVehicleStatusCommand : ICommand
    {
        private readonly AppDbContext _context;
        private readonly TextReader _reader;
        private readonly TextWriter _writer;

        public UpdateVehicleStatusCommand(AppDbContext context, TextReader reader, TextWriter writer)
        {
            _context = context;
            _reader = reader;
            _writer = writer;
        }

        public CommandResult Execute()
        {
            _writer.WriteLine("--- Update Vehicle Status ---");
            _writer.Write("Enter Car ID: ");

            if (int.TryParse(_reader.ReadLine(), out int id))
            {
                var car = _context.Cars.Find(id);
                if (car != null) // null check
                {
                    _writer.WriteLine($"Current Status: {car.Status}");
                    _writer.Write("Enter New Status (Available/Maintenance/Booked): ");
                    string newStatus = _reader.ReadLine();

                    if (!string.IsNullOrWhiteSpace(newStatus))
                    {
                        car.Status = newStatus;
                        _context.SaveChanges();
                        _writer.WriteLine("Status updated successfully.");
                    }
                    else
                    {
                        _writer.WriteLine("Status cannot be empty.");
                    }
                }
                else
                {
                    _writer.WriteLine("Car not found.");
                }
            }
            else
            {
                _writer.WriteLine("Invalid ID format.");
            }

            return CommandResult.CONTINUE;
        }

        public string Name() => "update-vehicle-status";
    }
}

