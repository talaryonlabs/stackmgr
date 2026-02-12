using System.CommandLine;

namespace stackmgr.Options;

public class EnableAutoSyncOption : Option<bool>
{
    public EnableAutoSyncOption() : base("--enable-auto-sync")
    {
        Description = "Enable/disable auto-sync";
    }
}