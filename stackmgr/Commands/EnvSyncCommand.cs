using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvSyncCommand : Command
{
    public EnvSyncCommand() : base("sync", "Sync environment with RKE2 (namespace)")
    {
        Add(new EnvironmentArgument());
        SetAction(v =>
        {
            
        });
    }
}