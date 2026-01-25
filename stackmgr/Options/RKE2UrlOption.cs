using System.CommandLine;

namespace stackmgr.Options;

public class RKE2UrlOption : Option<string>
{
    public RKE2UrlOption() : base("--rke2-url")
    {
        Description = "RKE2 API URL (e.g. https://rancher-url/v3)";
    }
}