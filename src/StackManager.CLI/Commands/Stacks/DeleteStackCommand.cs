using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for deleting a stack.
/// </summary>
public class DeleteStackCommand : ResourceDeleteCommand<Stack, StackArgument>
{
    public DeleteStackCommand()
        : base("stack", "Delete a stack")
    {
        Add(new EnvironmentOption());
    }

    protected override Stack LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var name = GetName<StackArgument>(parseResult);
        return env.GetStack(name);
    }

    protected override void DeleteResourceInstance(Stack resource)
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

    protected override void OnResourceDeleted(Stack resource)
    {
        LogMessage.AsSuccess("Success.");
    }
}
