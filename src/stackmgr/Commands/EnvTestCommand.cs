using stackmgr.Arguments;
using stackmgr.Services;

namespace stackmgr.Commands;

public class EnvTestCommand : StackManagerCommand
{
    public EnvTestCommand() : base("test", "Test an environment connection (RKE2, ArgoCD)")
    {
        Add(new EnvironmentArgument());
        SetAction(async v =>
        {
            var env = GetEnvironment<EnvironmentArgument>(v);
            
            Console.WriteLine($"Testing environment '{env.Name}' ...");
            
            Console.WriteLine(".. Testing RKE2 connection ...");
            await RKE2.TestConnection(env);
            
            Console.WriteLine(".. Testing ArgoCD connection ...");
            await ArgoCD.TestConnection(env);
        });
    }
}