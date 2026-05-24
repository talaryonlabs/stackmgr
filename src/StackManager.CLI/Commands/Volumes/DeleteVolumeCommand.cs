using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Volumes;

/// <summary>
/// Command for deleting a volume.
/// </summary>
public class DeleteVolumeCommand : ResourceDeleteCommand<StackVolume, VolumeArgument>
{
    public DeleteVolumeCommand()
        : base("volume", "Delete a volume")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackVolume LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<VolumeArgument>(parseResult);
        
        return stack.Get<StackVolume>(name);
    }

    protected override void DeleteResourceInstance(StackVolume resource)
    {
        var stack = resource.Stack;
        LogBuilder.Question($"Do you really want to delete volume '{resource.Name}'?")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                resource.Delete<StackVolume>();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    protected override void OnResourceDeleted(StackVolume resource)
    {
    }
}
