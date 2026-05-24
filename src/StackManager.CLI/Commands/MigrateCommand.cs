using System.Reflection;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class MigrateCommand : BaseCommand
{
    public MigrateCommand() : base("migrate", "Migrate a resource (app, image)")
    {
        // Auto-discover and add all ResourceMigrateCommand<TResource, TArg> implementations
        var migrateCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceMigrateCommand<,>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in migrateCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}