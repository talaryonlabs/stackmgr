using System.CommandLine;

namespace Talaryon.StackManager.Commands;

public class DefaultCommand : BaseCommand
{
    public DefaultCommand() : base("default", "Set default stack and environment for this session")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override void Execute()
    {
        var conf = GetRequiredService<LocalConfig>();
        var env = GetValue<string, EnvironmentOption>();
        var stack = GetValue<string, StackOption>();

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
