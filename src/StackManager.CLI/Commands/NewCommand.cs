using Talaryon.StackManager.Commands.Resources;


namespace Talaryon.StackManager.Commands;

public class NewCommand : BaseCommand
{
    public NewCommand() : base("new", "Create a new resource (environment, stack, app)")
    {
        // Auto-discover and add all ResourceCreateCommand<TResource, TArg> implementations
        UseAutodiscoverCommands(typeof(ResourceCreateCommand<,>));
    }
}
