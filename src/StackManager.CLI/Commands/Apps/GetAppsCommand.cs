using Talaryon.StackManager.Commands.Resources;

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

    protected override IReadOnlyList<StackApp> GetResources()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        return stack.Apps;
    }

    protected override void DisplayResource(StackApp resource)
    {
        LogMessage.AsSuccess($"- {resource.Name}");
    }
}
