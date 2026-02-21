using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RancherProjectIdOption : Option<string>
{
    public RancherProjectIdOption() : base("--rke2-project-id")
    {
        Description = "RKE2 project ID";
    }
}