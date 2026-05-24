using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Ingresses;

/// <summary>
/// Command for describing a single ingress.
/// </summary>
public class DescribeIngressCommand : ResourceDescribeCommand<StackIngress, HostnameArgument>
{
    public DescribeIngressCommand()
        : base("ingress", "Describe an ingress")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackIngress LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var hostname = GetName<HostnameArgument>(parseResult);
        var name = HelperMethods.HostToName(hostname);
        
        return stack.Get<StackIngress>(name);
    }

    protected override void DisplayResource(StackIngress resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("Ingress: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Hostname}").AsSuccess())
            .Run();

        LogBuilder.Message(" Application: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Application}").AsWarning())
            .Run();

        LogBuilder.Message(" Port: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Port}").AsWarning())
            .Run();

        LogBuilder.Message(" Secured: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.IsSecured}").AsWarning())
            .Run();

        LogMessage.Separator();
    }
}
