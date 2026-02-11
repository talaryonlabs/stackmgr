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
            
            Console.WriteLine($"Testing environment '{env.Name}' ...");
            
            Console.WriteLine(".. Testing RKE2 connection ...");
            await Rancher.TestConnection(env);
            
            Console.WriteLine(".. Testing ArgoCD connection ...");
            await Argo.TestConnection(env);
        });
    }
}