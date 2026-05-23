using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for describing a single stack.
/// </summary>
public class DescribeStackCommand : ResourceDescribeCommand<Talaryon.StackManager.Types.Stack, StackArgument>
{
    public DescribeStackCommand()
        : base("stack", "Describe a stack")
    {
        Add(new EnvironmentOption());
    }

    protected override Talaryon.StackManager.Types.Stack LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var name = GetName<StackArgument>(parseResult);
        return Talaryon.StackManager.Types.Stack.Load(env, name);
    }

    protected override void DisplayResource(Talaryon.StackManager.Types.Stack resource)
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
