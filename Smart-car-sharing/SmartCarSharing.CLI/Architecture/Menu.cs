using System;
using System.Collections.Generic;
using System.IO;

namespace SmartCarSharing.CLI.Architecture
{
    public class Menu : ICommand
    {
        private readonly string _name;
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly Dictionary<string, ICommand> _commands = new();

        public Menu(string name, TextReader input, TextWriter output)
        {
            _name = name;
            _input = input;
            _output = output;
        }

        public void Add(ICommand command)
        {
            _commands[command.Name()] = command;
        }

        public CommandResult Execute()
        {
            if (_commands.Count == 0)
            {
                _output.WriteLine("Menu is empty. Returning.");
                return CommandResult.CONTINUE;
            }

            CommandResult result;
            do
            {
                result = CommandResult.CONTINUE;
                Prompt();

                string commandName = _input.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(commandName)) continue;

                if (_commands.TryGetValue(commandName, out var command))
                {
                    result = command.Execute();
                }
                else
                {
                    _output.WriteLine("Command not found. Try again.");
                }

            } while (result == CommandResult.CONTINUE);

            return result == CommandResult.EXIT ? CommandResult.EXIT : CommandResult.CONTINUE;
        }

        public string Name() => _name;

        private void Prompt()
        {
            _output.WriteLine($"\nEnter one of the commands: {string.Join(", ", _commands.Keys)}");
            _output.Write(">");
        }
    }
}