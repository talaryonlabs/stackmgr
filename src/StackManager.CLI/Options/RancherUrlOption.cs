using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RancherUrlOption : Option<string>
{
    public RancherUrlOption() : base("--rke2-url")
    {
        Description = "RKE2 API URL (e.g. https://rancher-url)";
    }
}