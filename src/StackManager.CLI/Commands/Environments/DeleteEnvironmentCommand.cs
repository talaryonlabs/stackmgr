using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Environments;

/// <summary>
/// Command for deleting an environment.
/// </summary>
public class DeleteEnvironmentCommand : ResourceDeleteCommand<StackEnvironment, EnvironmentArgument>
{
    public DeleteEnvironmentCommand() 
        : base("environment", "Delete a environment")
    {
        Aliases.Add("env");
    }

    protected override StackEnvironment LoadResource(ParseResult parseResult)
    {
        return GetEnvironment<EnvironmentArgument>(parseResult);
    }

    protected override void DeleteResourceInstance(StackEnvironment resource)
    {
        LogMessage.AsWarning("ATTENTION: This only marks the environment 'deleted' in the config file.");
        LogMessage.AsWarning("It does not delete the environment from Rancher/ArgoCD nor the local directory.");
        LogMessage.AsWarning("");

        if (!LogMessage.AsConfirmWarning($"Are you sure you want to delete environment '{resource.Name}'?"))
        {
            LogMessage.AsInfo("Aborted.");
            return;
        }
        
        LogMessage.AsInfo($"Removing environment '{resource.Name}'.");
        resource.IsDeleted = true;
        resource.SaveConfig();
    }

    protected override void OnResourceDeleted(StackEnvironment resource)
    {
        LogMessage.AsSuccess("Success.");
    }
}
