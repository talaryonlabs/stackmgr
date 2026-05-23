using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Ingresses;

/// <summary>
/// Command for creating a new ingress.
/// </summary>
public class NewIngressCommand : ResourceCreateCommand<StackIngress, HostnameArgument>
{
    public NewIngressCommand()
        : base("ingress", "Create an ingress for an application")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new PortOption());
        Add(new AppOption());
        Add(new AnnotationOption());
        Add(new SecuredOption());
        Add(new GenerateOption());
    }

    protected override StackIngress CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var hostname = GetName<HostnameArgument>(parseResult);
        var app = parseResult.GetRequiredValue<string, AppOption>();
        var port = parseResult.GetRequiredValue<int, PortOption>();
        
        ValidationHelper.ValidateHostname(hostname);
        ValidationHelper.ValidateAppName(app);
        ValidationHelper.ValidatePort(port);
        
        if(parseResult.GetValue<bool, GenerateOption>())
        {
            if(hostname.StartsWith("."))
                hostname = hostname[1..];

            for (var i = 0; i < 10; i++)
            {
                var generated = $"{HelperMethods.GenerateRandomHostname()}-{stack.Name.ToLower()}.{hostname}";
                if (stack.Ingresses.Count(v =>
                        v.Hostname.Equals(generated, StringComparison.OrdinalIgnoreCase)) == 0)
                {
                    hostname = generated;
                    break;
                }
            }
        }

        return StackIngress.Create(stack, hostname, app, port, parseResult.GetValue<bool, SecuredOption>());
    }

    protected override void OnResourceCreated(StackIngress resource)
    {
        LogMessage.AsSuccess($"Ingress '{resource.Hostname}' created.");
    }
}
