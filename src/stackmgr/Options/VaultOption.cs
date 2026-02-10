using System.CommandLine;

namespace stackmgr.Options;

public class VaultOption : Option<string>
{
    public VaultOption() : base("--vault")
    {
        Description = "vault path for this stack (e.g. kv-stack/data/stackmgr/dev)";
    }
}