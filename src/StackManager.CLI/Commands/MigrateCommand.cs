using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class MigrateCommand : BaseCommand
{
    public MigrateCommand() : base("migrate", "Migrate a resource (app, image)")
    {
        // Auto-discover and add all ResourceMigrateCommand<TResource, TArg> implementations
        UseAutodiscoverCommands(typeof(ResourceMigrateCommand<,>));
    }
}