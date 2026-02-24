using System.CommandLine;
using Talaryon.StackManager.Arguments;

namespace Talaryon.StackManager.Commands;

public class DefaultCommand : StackManagerCommand
{
    public DefaultCommand() : base("default", "Defaults for this session")
    {
        var env = new StackManagerCommand("environment", "Set the current environment")
        {
            new EnvironmentArgument()
        };
        env.Aliases.Add("env");
        env.SetAction(SetEnvironment);

        var stack = new StackManagerCommand("stack", "Set the current stack")
        {
             new StackArgument()
        };
        stack.SetAction(SetStack);
        
        SetAction(v =>
        {
            HelperMethods.LogInfo($"Default environment: {Environment.GetEnvironmentVariable("STACKMGR_ENV", EnvironmentVariableTarget.User)}");
            HelperMethods.LogInfo($"Default stack: {Environment.GetEnvironmentVariable("STACKMGR_STACK", EnvironmentVariableTarget.User)}");
        });
        Add(env);
        Add(stack);
    }

    private void SetEnvironment(ParseResult parseResult)
    {
        var env = parseResult.GetRequiredValue<string, EnvironmentArgument>();
        Environment.SetEnvironmentVariable("STACKMGR_ENV", env, EnvironmentVariableTarget.User);
        HelperMethods.LogSuccess($"Environment set to '{env}'.");
        HelperMethods.LogInfo("Use --environment,--env to override this value per command.");
    }
    
    private void SetStack(ParseResult parseResult)
    {
        var stack = parseResult.GetRequiredValue<string, StackArgument>();
        Environment.SetEnvironmentVariable("STACKMGR_STACK", stack, EnvironmentVariableTarget.User);
        HelperMethods.LogSuccess($"Default stack set to '{stack}'.");
        HelperMethods.LogInfo("Use --stack to override this value per command.");
    }
}