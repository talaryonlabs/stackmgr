using System.Reflection;
using Talaryon.StackManager.Commands.Resources;


namespace Talaryon.StackManager.Commands;

public class DeleteCommand : BaseCommand
{
    public DeleteCommand() : base("delete", "Delete a resource (environment, stack, app)")
    {
        // Auto-discover and add all ResourceDeleteCommand<TResource, TArg> implementations
        UseAutodiscoverCommands(typeof(ResourceDeleteCommand<,>));
    }
}
