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
            LogMessage.AsInfo($"Default environment: {Environment.GetEnvironmentVariable("STACKMGR_ENV", EnvironmentVariableTarget.User)}");
            LogMessage.AsInfo($"Default stack: {Environment.GetEnvironmentVariable("STACKMGR_STACK", EnvironmentVariableTarget.User)}");
        });
        Add(env);
        Add(stack);
    }

    private void SetEnvironment(ParseResult parseResult)
    {
        var env = parseResult.GetRequiredValue<string, EnvironmentArgument>();
        Environment.SetEnvironmentVariable("STACKMGR_ENV", env, EnvironmentVariableTarget.User);
        LogMessage.AsSuccess($"Environment set to '{env}'.");
        LogMessage.AsInfo("Use --environment,--env to override this value per command.");
    }
    
    private void SetStack(ParseResult parseResult)
    {
        var stack = parseResult.GetRequiredValue<string, StackArgument>();
        Environment.SetEnvironmentVariable("STACKMGR_STACK", stack, EnvironmentVariableTarget.User);
        LogMessage.AsSuccess($"Default stack set to '{stack}'.");
        LogMessage.AsInfo("Use --stack to override this value per command.");
    }
}