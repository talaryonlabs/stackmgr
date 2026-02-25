namespace Talaryon.StackManager.Commands;

public class RemoteCommand : StackManagerCommand
{
    public RemoteCommand() : base("remote", "Manage remote proxy")
    {
        var add = new StackManagerCommand("add", "Add a remote proxy");
        
        var remove = new StackManagerCommand("remove", "Remove a remote proxy");
        
        Add(add);
        Add(remove);
    }
}