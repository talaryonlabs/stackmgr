using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
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

    protected override StackIngress LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var hostname = GetName<HostnameArgument>();
        var name = HelperMethods.HostToName(hostname);
        
        return stack.Get<StackIngress>(name);
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
                resource.Delete<StackIngress>();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    protected override void OnResourceDeleted(StackIngress resource)
    {
    }
}
