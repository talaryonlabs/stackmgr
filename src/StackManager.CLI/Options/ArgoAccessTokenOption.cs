using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ArgoAccessTokenOption : Option<string>
{
    public ArgoAccessTokenOption() : base("--argocd-access-token")
    {
        Description = "ArgoCD access token";
    }   
}