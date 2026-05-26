using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for configuring a stack.
/// </summary>
public class ConfigureStackCommand : ResourceConfigureCommand<StackArgument>
{
    public ConfigureStackCommand()
        : base("stack", "Configure a stack")
    {
        Add(new EnvironmentOption { Required = true });
        Add(new EnableAutoSyncOption());
    }

    protected override void Configure()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackArgument>(env);

        if (ParseResult.Tokens.Any(v => v.Value == "--enable-auto-sync"))
        {
            stack.EnableAutoSync = GetValue<bool, EnableAutoSyncOption>();
        }
        
        stack.Save();
    }
}
