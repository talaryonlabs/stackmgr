using System.CommandLine;

namespace stackmgr.Options;

public class ArgoAccessTokenOption : Option<string>
{
    public ArgoAccessTokenOption() : base("--argocd-access-token")
    {
        Description = "ArgoCD access token";
    }   
}