using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class TestCommand : StackManagerCommand
{
    public TestCommand() : base("test", "Test an environment connection (RKE2, ArgoCD)")
    {
        Add(new EnvironmentArgument());
        SetAction(async parseResult =>
        {
            var env = GetEnvironment<EnvironmentArgument>(parseResult);
            using var argo = new ArgoService(env);
            using var rancher = new RancherService(env);
            
            LogMessage.AsInfo($"Testing environment '{env.Name}' ...");
            
            LogMessage.AsInfo(".. Testing RKE2 connection ...");
            await rancher.TestAsync();
            LogMessage.AsSuccess("Done.");
            
            LogMessage.AsInfo(".. Testing ArgoCD connection ...");
            await argo.TestAsync();
            LogMessage.AsSuccess("Done.");
        });
    }
}