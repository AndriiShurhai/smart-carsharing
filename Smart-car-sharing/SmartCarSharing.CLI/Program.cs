using System;
using Microsoft.EntityFrameworkCore;
using SmartCarSharing.CLI.Architecture;
using SmartCarSharing.CLI.Commands;
using SmartCarSharing.Data;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Smart Car Sharing CLI...");

        // 1. Setup Database 
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=../../../../smartcarsharing.db")
            .Options;

        using var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        // 2. Input/Output
        var input = Console.In;
        var output = Console.Out;

        // 3. Build the Menu (Composite Pattern)
        var mainMenu = new Menu("main", input, output);

        // Add basic navigation commands
        mainMenu.Add(new ExitCommand());

        // Add Admin Commands
        mainMenu.Add(new ListUsersCommand(context, output));
        mainMenu.Add(new ListCarsCommand(context, output));
        mainMenu.Add(new StatsCommand(context, output));

        // Add Vehicle Management Commands
        mainMenu.Add(new AddVehicleCommand(context, input, output));
        mainMenu.Add(new RemoveVehicleCommand(context, input, output));
        mainMenu.Add(new UpdateVehicleStatusCommand(context, input, output));


        // 4. Run the Shell
        mainMenu.Execute();
    }
}

public class ExitCommand : ICommand
{
    public CommandResult Execute() => CommandResult.EXIT;
    public string Name() => "exit";
}