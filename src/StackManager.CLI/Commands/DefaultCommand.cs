using System.CommandLine;
using Talaryon.StackManager.Options;

namespace Talaryon.StackManager.Commands;

public class DefaultCommand : StackManagerCommand
{
    private readonly LocalConfig _conf;

    public DefaultCommand() : base("default", "Set default stack and environment for this session")
    {
        _conf = LocalConfig.Get();
        Add(new EnvironmentOption());
        Add(new StackOption());
        SetAction(SetDefaults);
    }

    private void SetDefaults(ParseResult parseResult)
    {
        var env = parseResult.GetValue<string, EnvironmentOption>();
        var stack = parseResult.GetValue<string, StackOption>();

        if (env is not null)
        {
            _conf.Defaults.Environment = env;
            LogMessage.AsSuccess($"Default environment set to '{env}'.");
        }

        if (stack is not null)
        {
            _conf.Defaults.Stack = stack;
            LogMessage.AsSuccess($"Default stack set to '{stack}'.");
        }

        _conf.Save();

        if (env is null && stack is null)
        {
            LogMessage.AsInfo($"\n  Environment: {_conf.Defaults.Environment ?? "(not set)"}\n  Stack: {_conf.Defaults.Stack ?? "(not set)"}");
        }
    }
}
