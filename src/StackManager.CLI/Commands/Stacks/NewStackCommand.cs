using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for creating a new stack.
/// </summary>
public class NewStackCommand : ResourceCreateCommand<Stack, StackArgument>
{
    public NewStackCommand()
        : base("stack", "Create a new stack")
    {
        Add(new EnvironmentOption());
        Add(new TemplateOption());
    }

    protected override Stack CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var name = GetName<StackArgument>(parseResult);
        ValidationHelper.ValidateStackName(name);
        
        return env.NewStack(name);
    }

    protected override void OnResourceCreated(Stack resource)
    {
        LogMessage.AsSuccess($"Stack '{resource.Name}' created.");
    }
}
