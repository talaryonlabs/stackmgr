using System.Reflection;
using Talaryon.StackManager.Commands.Resources;


namespace Talaryon.StackManager.Commands;

public class NewCommand : BaseCommand
{
    public NewCommand() : base("new", "Create a new resource (environment, stack, app)")
    {
        // Auto-discover and add all ResourceCreateCommand<TResource, TArg> implementations
        var createCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceCreateCommand<,>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in createCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}
