using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDNamespaceOption : Option<string>
{
    public ArgoCDNamespaceOption() : base("--argocd-namespace")
    {
        Description = "ArgoCD namespace (e.g. argocd)";
    }   
}