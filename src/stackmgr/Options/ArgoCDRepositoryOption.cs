using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDRepositoryOption : Option<string>
{
    public ArgoCDRepositoryOption() : base("--argocd-repository")
    {
        Description = "ArgoCD repository name (e.g. https://github.com/argoproj/argo-cd)";
    }  
}