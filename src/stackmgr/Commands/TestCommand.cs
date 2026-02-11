using stackmgr.Arguments;
using stackmgr.Services;

namespace stackmgr.Commands;

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
            
            HelperMethods.LogInfo($"Testing environment '{env.Name}' ...");
            
            HelperMethods.LogInfo(".. Testing RKE2 connection ...");
            await rancher.TestAsync();
            HelperMethods.LogSuccess("Done.");
            
            HelperMethods.LogInfo(".. Testing ArgoCD connection ...");
            await argo.TestAsync();
            HelperMethods.LogSuccess("Done.");
        });
    }
}