using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for describing a single stack.
/// </summary>
public class DescribeStackCommand : ResourceDescribeCommand<Stack, StackArgument>
{
    public DescribeStackCommand()
        : base("stack", "Describe a stack")
    {
        Add(new EnvironmentOption());
    }

    protected override Stack LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var name = GetName<StackArgument>();
        return env.GetStack(name);
    }

    protected override void DisplayResource(Stack resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("Stack: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name}").AsColored(ConsoleColor.Cyan))
            .Run();

        LogBuilder.Message("Environment: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Environment.Name}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Namespace: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Namespace}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Version: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Version ?? "(default)"}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("AutoSync: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.EnableAutoSync}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogMessage.Separator();

        if (resource.Apps.Count > 0)
        {
            LogMessage.AsInfo("Apps:");
            foreach (var app in resource.Apps)
            {
                LogMessage.AsSuccess($"  - {app.Name}");
            }
        }

        if (resource.Images.Count > 0)
        {
            LogMessage.AsInfo("Images:");
            foreach (var image in resource.Images)
            {
                LogMessage.AsSuccess($"  - {image.Name}: {image.Image}");
            }
        }

        if (resource.Volumes.Count > 0)
        {
            LogMessage.AsInfo("Volumes:");
            foreach (var volume in resource.Volumes)
            {
                LogMessage.AsSuccess($"  - {volume.Name}: {volume.StorageSize} ({volume.AccessMode})");
            }
        }

        if (resource.Ingresses.Count > 0)
        {
            LogMessage.AsInfo("Ingresses:");
            foreach (var ingress in resource.Ingresses)
            {
                LogMessage.AsSuccess($"  - {ingress.Hostname} [{ingress.Application}]");
            }
        }

        LogMessage.Separator();
    }
}
