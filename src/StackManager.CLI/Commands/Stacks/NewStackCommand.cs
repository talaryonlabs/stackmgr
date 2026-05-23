using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Validation;
using Talaryon.Toolbox;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for creating a new stack.
/// </summary>
public class NewStackCommand : ResourceCreateCommand<Talaryon.StackManager.Types.Stack, StackArgument>
{
    public NewStackCommand()
        : base("stack", "Create a new stack")
    {
        Add(new EnvironmentOption());
        Add(new TemplateOption());
    }

    protected override Talaryon.StackManager.Types.Stack CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var name = GetName<StackArgument>(parseResult);
        ValidationHelper.ValidateStackName(name);
        return Talaryon.StackManager.Types.Stack.Create(env, name);
    }

    protected override void OnResourceCreated(Talaryon.StackManager.Types.Stack resource)
    {
        LogMessage.AsSuccess($"Stack '{resource.Name}' created.");
    }
}
