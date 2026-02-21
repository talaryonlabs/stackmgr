using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class EnableAutoSyncOption : Option<bool>
{
    public EnableAutoSyncOption() : base("--enable-auto-sync")
    {
        Description = "Enable/disable auto-sync";
    }
}