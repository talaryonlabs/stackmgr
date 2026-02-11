using System.CommandLine;

namespace stackmgr.Options;

public class NameOption : Option<string>
{
    public NameOption() : base("--name")
    {
        Description = "image name (e.g. nginx)";
    }
}