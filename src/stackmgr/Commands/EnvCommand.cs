namespace stackmgr.Commands;

public class EnvCommand : StackManagerCommand
{
    public EnvCommand() : base("env", "Manage environments")
    {
        Aliases.Add("environment");
        Add(new EnvInitCommand());
        Add(new EnvConfigureCommand());
        Add(new EnvDropCommand());
        Add(new EnvTestCommand());
    }
}