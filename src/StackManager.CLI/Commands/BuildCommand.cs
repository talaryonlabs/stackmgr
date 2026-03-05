using Talaryon.StackManager.Options;

namespace Talaryon.StackManager.Commands;

public class BuildCommand : StackManagerCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        SetAction(async parseResult =>
        {
            var env = GetEnvironment<EnvironmentOption>(parseResult);
            var stack = GetStack<StackOption>(parseResult, env);
            
            await stack.Build();
            LogMessage.AsSuccess($"Stack '{stack.Name}' built.");
            LogMessage.AsInfo("Run git commit and git push before stack sync.");
        });
    }
}