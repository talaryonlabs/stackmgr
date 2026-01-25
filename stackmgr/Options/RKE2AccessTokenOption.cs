using System.CommandLine;

namespace stackmgr.Options;

public class RKE2AccessTokenOption : Option<string>
{
    public RKE2AccessTokenOption() : base("--rke2-access-token")
    {
        Description = "RKE2 access token";
    }
}