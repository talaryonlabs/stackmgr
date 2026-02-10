using System.CommandLine;

namespace stackmgr.Options;

public class ArgoRepositoryOption : Option<string>
{
    public ArgoRepositoryOption() : base("--argocd-repository")
    {
        Description = "ArgoCD repository name (e.g. https://github.com/argoproj/argo-cd)";
    }  
}