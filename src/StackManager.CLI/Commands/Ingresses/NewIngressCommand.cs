using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
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

    protected override StackIngress CreateResourceInstance()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var hostname = GetName<HostnameArgument>();
        var name = HelperMethods.HostToName(hostname);
        var app = GetRequiredValue<string, AppOption>();
        var port = GetRequiredValue<int, PortOption>();
        
        ValidationHelper.ValidateHostname(hostname);
        ValidationHelper.ValidateAppName(app);
        ValidationHelper.ValidatePort(port);
        
        if(GetValue<bool, GenerateOption>())
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

        return stack
            .New<StackIngress>()
            .WithName(name)
            .Configure(ingress =>
            {
                ingress.Hostname = hostname;
                ingress.Port = port;
                ingress.Application = app;
                ingress.IsSecured = GetValue<bool, SecuredOption>();
            })
            .Save();
    }

    protected override void OnResourceCreated(StackIngress resource)
    {
        LogMessage.AsSuccess($"Ingress '{resource.Hostname}' created.");
    }
}
