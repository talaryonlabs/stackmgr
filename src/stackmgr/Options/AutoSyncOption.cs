using System.CommandLine;

namespace stackmgr.Options;

public class AutoSyncOption : Option<bool>
{
    public AutoSyncOption() : base("--auto-sync")
    {
        Description = "Enable/disable auto-sync";
    }
}