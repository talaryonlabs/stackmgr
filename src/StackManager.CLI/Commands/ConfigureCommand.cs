using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class ConfigureCommand : BaseCommand
{
    public ConfigureCommand() : base("configure", "Configure a resource (environment, stack, app, global)")
    {
        // Auto-discover and add all ResourceConfigureCommand<TArg> implementations
        UseAutodiscoverCommands(typeof(ResourceConfigureCommand<>));
    }
}
