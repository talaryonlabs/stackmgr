using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackDisableCommand : StackManagerCommand
{
    public StackDisableCommand() : base("disable", "Disable a stack")
    {
        SetAction(async v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);
            
            _ = await ArgoCD.DisableAutoSync(env, stack.Namespace);
            
            Console.WriteLine($"Disabling stack '{stack.Name}' in environment '{env.Name}'");
        });
    }
}