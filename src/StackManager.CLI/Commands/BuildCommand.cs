using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class BuildCommand : BaseCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        SetAction(async parseResult =>
        {
            var env = GetEnvironment<EnvironmentOption>(parseResult);
            var stack = GetStack<StackOption>(parseResult, env);
            var kustomizeService = GetService<KustomizeService>();
            var builder = new StackBuilder(stack).WithKustomizeValidation(kustomizeService);
            
            await builder.BuildAsync();
            LogMessage.AsSuccess($"Stack '{stack.Name}' built.");
        });
    }
}