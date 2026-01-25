using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDServiceOption : Option<string>
{
    public ArgoCDServiceOption() : base("--argocd-service")
    {
        Description = "ArgoCD service name (e.g. argocd-server)";
    }   
}