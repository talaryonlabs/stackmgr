using System.CommandLine;
using Talaryon.StackManager.Options;

namespace Talaryon.StackManager.Commands;

public class DefaultCommand : StackManagerCommand
{
    public DefaultCommand() : base("default", "Set default stack and environment for this session")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        SetAction(SetDefaults);
    }

    private void SetDefaults(ParseResult parseResult)
    {
        var conf = GetRequiredService<LocalConfig>();
        var env = parseResult.GetValue<string, EnvironmentOption>();
        var stack = parseResult.GetValue<string, StackOption>();

        if (env is not null)
        {
            conf.Defaults.Environment = env;
            LogMessage.AsSuccess($"Default environment set to '{env}'.");
        }

        if (stack is not null)
        {
            conf.Defaults.Stack = stack;
            LogMessage.AsSuccess($"Default stack set to '{stack}'.");
        }

        conf.Save();

        if (env is null && stack is null)
        {
            LogMessage.AsInfo($"\n  Environment: {conf.Defaults.Environment ?? "(not set)"}\n  Stack: {conf.Defaults.Stack ?? "(not set)"}");
        }
    }
}
