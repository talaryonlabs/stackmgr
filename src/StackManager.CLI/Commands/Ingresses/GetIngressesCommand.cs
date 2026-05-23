using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

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

    protected override IReadOnlyList<StackIngress> GetResources(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        return stack.Ingresses;
    }

    protected override void DisplayResource(StackIngress resource)
    {
        LogMessage.AsSuccess($"- {resource.Hostname} [{resource.Application}]");
    }
}
