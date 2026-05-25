using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class DescribeCommand : BaseCommand
{
    public DescribeCommand() : base("describe", "Describe a resource (environment, stack, template)")
    {
        // Auto-discover and add all ResourceDescribeCommand<TResource, TArg> implementations
        UseAutodiscoverCommands(typeof(ResourceDescribeCommand<,>));
    }
}
