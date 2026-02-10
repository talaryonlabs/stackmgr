using System.CommandLine;

namespace stackmgr.Options;

public class ArgoProjectOption : Option<string>
{
    public ArgoProjectOption() : base("--argocd-project")
    {
        Description = "ArgoCD project name (e.g. default)";
    }
}