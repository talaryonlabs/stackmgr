using System.Reflection;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands;

public class DescribeCommand : BaseCommand
{
    public DescribeCommand() : base("describe", "Describe a resource (environment, stack, template)")
    {
        // Auto-discover and add all ResourceDescribeCommand<TResource, TArg> implementations
        var describeCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceDescribeCommand<,>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in describeCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }
    }
}
