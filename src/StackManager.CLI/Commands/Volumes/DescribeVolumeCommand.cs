using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Volumes;

/// <summary>
/// Command for describing a single volume.
/// </summary>
public class DescribeVolumeCommand : ResourceDescribeCommand<StackVolume, VolumeArgument>
{
    public DescribeVolumeCommand()
        : base("volume", "Describe a volume")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackVolume LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var name = GetName<VolumeArgument>();
        
        return stack.Get<StackVolume>(name);
    }

    protected override void DisplayResource(StackVolume resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("Volume: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name}").AsSuccess())
            .Run();

        LogBuilder.Message(" Storage Size: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.StorageSize}").AsWarning())
            .Run();

        LogBuilder.Message(" Access Mode: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.AccessMode}").AsWarning())
            .Run();

        LogMessage.Separator();
    }
}
