using System.CommandLine;

namespace stackmgr.Commands;

public class DefaultCommand : StackManagerCommand
{
    public DefaultCommand() : base("default", "Defaults for this session")
    {
        var env = new StackManagerCommand("environment", "Set the current environment")
        {
            new Argument<string>("environment") 
        };
        env.Aliases.Add("env");
        env.SetAction(SetEnvironment);
        
        SetAction(v =>
        {
            HelperMethods.LogInfo($"Default environemt: {Environment.GetEnvironmentVariable("STACKMGR_ENV", EnvironmentVariableTarget.User)}");
        });
        Add(env);
    }

    private void SetEnvironment(ParseResult parseResult)
    {
        var env = parseResult.GetValue<string>("environment");
        Environment.SetEnvironmentVariable("STACKMGR_ENV", env, EnvironmentVariableTarget.User);
        HelperMethods.LogSuccess($"Environment set to '{env}'.");
        HelperMethods.LogInfo("Use --environment,--env to override this value per command.");
    }
}