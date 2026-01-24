using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackEnableCommand : Command
{
    public StackEnableCommand() : base("enable", "Enable a stack")
    {
        SetAction(v =>
        {
            var env = v.GetRequiredValue<StackEnvironment, EnvironmentOption>();
            var p = v.GetRequiredValue<string, StackNameArgument>();

            if (!env.HasStack(p))
            {
                Console.WriteLine($"Stack '{p}' does not exist in environment '{env}'");
                return;
            }
            
            
            Console.WriteLine($"Enabling stack '{p}' in environment '{env}'");
        });
    }
}