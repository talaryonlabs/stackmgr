using System.CommandLine;

namespace stackmgr.Commands;

public class EnvCommand : Command
{
    public EnvCommand() : base("env", "Manage environments")
    {
        Aliases.Add("environment");
        Add(new EnvInitCommand());
        Add(new EnvConfigureCommand());
        Add(new EnvDropCommand());
        Add(new EnvSyncCommand());
    }
}