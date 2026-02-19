using System.CommandLine;

namespace stackmgr.Options;

public class OutpostOption : Option<string>
{
    public OutpostOption() : base("--outpost")
    {
        Description = "outpost name (e.g. authentik.authentik-namespace.svc.cluster.local)";
    }
}