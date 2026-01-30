using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDProjectOption : Option<string>
{
    public ArgoCDProjectOption() : base("--argocd-project")
    {
        Description = "ArgoCD project name (e.g. default)";
    }
}