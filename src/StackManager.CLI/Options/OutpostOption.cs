using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class OutpostOption : Option<string>
{
    public OutpostOption() : base("--outpost")
    {
        Description = "outpost name (e.g. authentik.authentik-namespace.svc.cluster.local)";
    }
}