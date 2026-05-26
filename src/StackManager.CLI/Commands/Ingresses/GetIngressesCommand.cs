using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Ingresses;

/// <summary>
/// Command for listing ingresses in a stack.
/// </summary>
public class GetIngressesCommand : ResourceGetCommand<StackIngress>
{
    public GetIngressesCommand()
        : base("ingresses", "List ingresses", "Ingresses", new EnvironmentOption(), new StackOption())
    {
    }

    protected override IReadOnlyList<StackIngress> GetResources()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        return stack.Ingresses;
    }

    protected override void DisplayResource(StackIngress resource)
    {
        LogMessage.AsSuccess($"- {resource.Hostname} [{resource.Application}]");
    }
}
