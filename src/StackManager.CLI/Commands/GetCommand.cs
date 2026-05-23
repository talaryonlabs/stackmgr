using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;


namespace Talaryon.StackManager.Commands;

public class GetCommand : StackManagerCommand
{
    public GetCommand() : base("get", "Get a resource")
    {
        // Auto-discover and add all ResourceGetCommand<T> implementations
        var getCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ResourceGetCommand<>)) && !t.IsAbstract && t != typeof(ResourceGetCommand<>))
            .ToList();

        foreach (var type in getCommandTypes)
        {
            var instance = (StackManagerCommand?)Activator.CreateInstance(type);
            if (instance != null)
            {
                Add(instance);
            }
        }
    }
}
