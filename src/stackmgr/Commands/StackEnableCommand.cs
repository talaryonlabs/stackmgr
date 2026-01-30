using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackEnableCommand : Command
{
    public StackEnableCommand() : base("enable", "Enable a stack")
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

            _ = await ArgoCD.EnableAutoSync(env, p);
            
            
            
            if (!env.HasStack(p))
            {
                Console.WriteLine($"Stack '{p}' does not exist in environment '{env.Name}'");
                return;
            }
            
            
            Console.WriteLine($"Enabling stack '{p}' in environment '{env.Name}'");
        });
    }
}