using System.CommandLine;

namespace stackmgr.Options;

public class RKE2ProjectIdOption : Option<string>
{
    public RKE2ProjectIdOption() : base("--rke2-project-id")
    {
        Description = "RKE2 project ID";
    }
}