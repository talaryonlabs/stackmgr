using System.Reflection;
using Talaryon.StackManager.Commands.Resources;


namespace Talaryon.StackManager.Commands;

public class DeleteCommand : BaseCommand
{
    public DeleteCommand() : base("delete", "Delete a resource (environment, stack, app)")
    {
        // Auto-discover and add all ResourceDeleteCommand<TResource, TArg> implementations
        var deleteCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceDeleteCommand<,>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in deleteCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}
