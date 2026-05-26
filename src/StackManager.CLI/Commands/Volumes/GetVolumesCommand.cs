using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Volumes;

/// <summary>
/// Command for listing volumes in a stack.
/// </summary>
public class GetVolumesCommand : ResourceGetCommand<StackVolume>
{
    public GetVolumesCommand()
        : base("volumes", "List volumes", "Volumes", new EnvironmentOption(), new StackOption())
    {
    }

    protected override IReadOnlyList<StackVolume> GetResources()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        return stack.Volumes;
    }

    protected override void DisplayResource(StackVolume resource)
    {
        LogMessage.AsSuccess($"- {resource.Name}: {resource.StorageSize} ({resource.AccessMode})");
    }
}
