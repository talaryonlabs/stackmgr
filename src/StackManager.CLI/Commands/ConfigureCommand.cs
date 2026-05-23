using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands;

public class ConfigureCommand : StackManagerCommand
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

        foreach (var type in configureCommandTypes)
        {
            var instance = (StackManagerCommand?)Activator.CreateInstance(type);
            if (instance != null)
            {
                Add(instance);
            }
        }
    }
}
