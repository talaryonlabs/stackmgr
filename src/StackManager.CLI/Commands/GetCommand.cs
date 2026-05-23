using System.Reflection;
using Talaryon.StackManager.Commands.Resources;


namespace Talaryon.StackManager.Commands;

public class GetCommand : BaseCommand
{
    public GetCommand() : base("get", "Get a resource")
    {
        // Auto-discover and add all ResourceGetCommand<T> implementations
        var getCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                        && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceGetCommand<>)
                        && !t.IsAbstract)
            .ToList();

        foreach (var instance in getCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}
