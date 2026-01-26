using System.CommandLine;
using System.Net;
using stackmgr.Arguments;
using stackmgr.Services;
using Talaryon.Toolbox.Extensions;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace stackmgr.Commands;

public class EnvTestCommand : Command
{
    public EnvTestCommand() : base("test", "Test an environment connection (RKE2, ArgoCD)")
    {
        Add(new EnvironmentArgument());
        SetAction(async v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            Console.WriteLine($"Testing environment '{env.Name}' ...");
            
            Console.WriteLine(".. Testing RKE2 connection ...");
            await RKE2.TestConnection(env);
            
            Console.WriteLine(".. Testing ArgoCD connection ...");
            await ArgoCD.TestConnection(env);
            
            await ArgoCD.DisableAutoSync(env, "ambulanz.care");
        });
    }
}