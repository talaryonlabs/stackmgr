using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackDisableCommand : Command
{
    public StackDisableCommand() : base("disable", "Disable a stack")
    {
        SetAction(async v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentOption>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            var p = v.GetRequiredValue<string, StackArgument>();

            _ = await ArgoCD.DisableAutoSync(env, p);
            
            
            
            if (!env.HasStack(p))
            {
                Console.WriteLine($"Stack '{p}' does not exist in environment '{env.Name}'");
                return;
            }
            
            
            Console.WriteLine($"Disabling stack '{p}' in environment '{env.Name}'");
        });
    }
}