namespace SmartCarSharing.CLI.Architecture
{
    public interface ICommand
    {
        CommandResult Execute();
        string Name();
    }
}