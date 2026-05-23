using System.Reflection;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class ConfigureCommand : BaseCommand
{
    public ConfigureCommand() : base("configure", "Configure a resource (environment, stack, app, global)")
    {
        // Auto-discover and add all ResourceConfigureCommand<TArg> implementations
        var configureCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceConfigureCommand<>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in configureCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}
