using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for describing a single app.
/// </summary>
public class DescribeAppCommand : ResourceDescribeCommand<StackApp, AppArgument>
{
    public DescribeAppCommand()
        : base("app", "Describe an application")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackApp LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<AppArgument>(parseResult);
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) 
            ?? throw new AppNotFoundException(name);
    }

    protected override void DisplayResource(StackApp resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("App: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name}").AsSuccess())
            .Run();

        if (resource.Template != null)
        {
            LogBuilder.Message(" Template: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"{resource.Template.Name} ({resource.Template.Branch})").AsWarning())
                .Run();
        }

        if (resource.Volumes.Count > 0)
        {
            LogBuilder.Message(" Volumes: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", resource.Volumes.Select(v => $"{v.Key}:{v.Value}"))}]").AsSuccess())
                .Run();
        }

        if (resource.Requirements.Count > 0)
        {
            LogBuilder.Message(" Requirements: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", resource.Requirements.Select(r => $"{r.Key}:{r.Value}"))}]").AsWarning())
                .Run();
        }

        if (resource.Params.Count > 0)
        {
            LogBuilder.Message(" Params: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", resource.Params.Select(p => $"{p.Key}:{p.Value}"))}]").AsSuccess())
                .Run();
        }

        LogMessage.Separator();
    }
}
