using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for deleting a stack.
/// </summary>
public class DeleteStackCommand : ResourceDeleteCommand<Talaryon.StackManager.Types.Stack, StackArgument>
{
    public DeleteStackCommand()
        : base("stack", "Delete a stack")
    {
        Add(new EnvironmentOption());
    }

    protected override Talaryon.StackManager.Types.Stack LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var name = GetName<StackArgument>(parseResult);
        return Talaryon.StackManager.Types.Stack.Load(env, name);
    }

    protected override void DeleteResourceInstance(Talaryon.StackManager.Types.Stack resource)
    {
        LogMessage.AsWarning("ATTENTION: This only marks the stack 'deleted' in the config file.");
        LogMessage.AsWarning("It does not delete the stack from Rancher/ArgoCD nor the local directory.");
        LogMessage.AsWarning("");

        if (!LogMessage.AsConfirmWarning($"Are you sure you want to delete stack '{resource.Name}'?"))
        {
            LogMessage.AsInfo("Aborted.");
            return;
        }
        
        LogMessage.AsInfo($"Removing stack '{resource.Name}'.");
        resource.Delete();
    }

    protected override void OnResourceDeleted(Talaryon.StackManager.Types.Stack resource)
    {
        LogMessage.AsSuccess("Success.");
    }
}
