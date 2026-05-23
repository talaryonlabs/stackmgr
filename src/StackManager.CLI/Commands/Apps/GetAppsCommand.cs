using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for listing apps in a stack.
/// </summary>
public class GetAppsCommand : ResourceGetCommand<StackApp>
{
    public GetAppsCommand()
        : base("apps", "List applications", "Apps", new EnvironmentOption(), new StackOption())
    {
    }

    protected override IReadOnlyList<StackApp> GetResources(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        return stack.Apps;
    }

    protected override void DisplayResource(StackApp resource)
    {
        LogMessage.AsSuccess($"- {resource.Name}");
    }
}
