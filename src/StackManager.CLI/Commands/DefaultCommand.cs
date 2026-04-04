using System.CommandLine;
using Talaryon.StackManager.Arguments;

namespace Talaryon.StackManager.Commands;

public class DefaultCommand : StackManagerCommand
{
    private readonly LocalConfig _conf;

    public DefaultCommand() : base("default", "Defaults for this session")
    {
        _conf = LocalConfig.Get();
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
            LogMessage.AsInfo($"Default environment: {_conf.Defaults.Environment}");
            LogMessage.AsInfo($"Default stack: {_conf.Defaults.Stack}");
        });
        Add(env);
        Add(stack);
    }

    private void SetEnvironment(ParseResult parseResult)
    {
        var env = parseResult.GetRequiredValue<string, EnvironmentArgument>();
        _conf.Defaults.Environment = env;
        _conf.Save();
        LogMessage.AsSuccess($"Environment set to '{env}'.");
        LogMessage.AsInfo("Use --environment,--env to override this value per command.");
    }
    
    private void SetStack(ParseResult parseResult)
    {
        var stack = parseResult.GetRequiredValue<string, StackArgument>();
        _conf.Defaults.Stack = stack;
        _conf.Save();
        LogMessage.AsSuccess($"Default stack set to '{stack}'.");
        LogMessage.AsInfo("Use --stack to override this value per command.");
    }
}