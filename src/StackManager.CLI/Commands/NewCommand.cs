using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Validation;


namespace Talaryon.StackManager.Commands;

public class NewCommand : StackManagerCommand
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

        foreach (var type in createCommandTypes)
        {
            var instance = (StackManagerCommand?)Activator.CreateInstance(type);
            if (instance != null)
            {
                Add(instance);
            }
        }
    }
}
