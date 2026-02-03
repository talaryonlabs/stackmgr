using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackEnableCommand : StackManagerCommand
{
    public StackEnableCommand() : base("enable", "Enable a stack")
    {
        SetAction(async v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);
            
            _ = await ArgoCD.EnableAutoSync(env, stack.Namespace);
            
            Console.WriteLine($"Enabling stack '{stack.Name}' in environment '{env.Name}'");
        });
    }
}