using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class BuildCommand : StackManagerCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackArgument());
        SetAction(v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);
            
            HelperMethods.LogInfo($"Building stack '{stack.Name}' in environment '{env.Name}'");
            stack.SaveKustomization();
            HelperMethods.LogSuccess($"Stack '{stack.Name}' built.");
            HelperMethods.LogInfo("Run git commit and git push before stack sync.");
        });
    }
}