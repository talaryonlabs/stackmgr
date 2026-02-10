using System.CommandLine;

namespace stackmgr.Options;

public class RancherAccessTokenOption : Option<string>
{
    public RancherAccessTokenOption() : base("--rke2-access-token")
    {
        Description = "RKE2 access token";
    }
}