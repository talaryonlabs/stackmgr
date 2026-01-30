using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDAccessTokenOption : Option<string>
{
    public ArgoCDAccessTokenOption() : base("--argocd-access-token")
    {
        Description = "ArgoCD access token";
    }   
}