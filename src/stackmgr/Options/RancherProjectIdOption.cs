using System.CommandLine;

namespace stackmgr.Options;

public class RancherProjectIdOption : Option<string>
{
    public RancherProjectIdOption() : base("--rke2-project-id")
    {
        Description = "RKE2 project ID";
    }
}