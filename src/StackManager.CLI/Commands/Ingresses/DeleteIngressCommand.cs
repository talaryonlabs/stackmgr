using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Ingresses;

/// <summary>
/// Command for deleting an ingress.
/// </summary>
public class DeleteIngressCommand : ResourceDeleteCommand<StackIngress, HostnameArgument>
{
    public DeleteIngressCommand()
        : base("ingress", "Delete an ingress")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackIngress LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var hostname = GetName<HostnameArgument>(parseResult);
        return stack.Ingresses.FirstOrDefault(v => v.Hostname.Equals(hostname, StringComparison.OrdinalIgnoreCase)) 
            ?? throw new IngressNotFoundException(hostname);
    }

    protected override void DeleteResourceInstance(StackIngress resource)
    {
        LogBuilder.Question($"Do you really want to delete ingress '{resource.Hostname}'?")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                resource.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    protected override void OnResourceDeleted(StackIngress resource)
    {
    }
}
