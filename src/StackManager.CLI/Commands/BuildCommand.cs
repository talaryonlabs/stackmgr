using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;

namespace Talaryon.StackManager.Commands;

public class BuildCommand : StackManagerCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackArgument());
        SetAction(async parseResult =>
        {
            var env = GetEnvironment<EnvironmentOption>(parseResult);
            var stack = GetStack<StackArgument>(parseResult, env);
            
            await stack.Build();
            HelperMethods.LogSuccess($"Stack '{stack.Name}' built.");
            HelperMethods.LogInfo("Run git commit and git push before stack sync.");
        });
    }
}