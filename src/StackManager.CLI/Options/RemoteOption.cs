using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RemoteOption : Option<string>
{
    public RemoteOption() : base("--remote")
    {
        Description = "remote name (e.g. dev)";
    }
}