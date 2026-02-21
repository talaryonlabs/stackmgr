using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ArgoProjectOption : Option<string>
{
    public ArgoProjectOption() : base("--argocd-project")
    {
        Description = "ArgoCD project name (e.g. default)";
    }
}