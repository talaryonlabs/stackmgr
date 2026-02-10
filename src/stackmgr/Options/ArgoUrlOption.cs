using System.CommandLine;

namespace stackmgr.Options;

public class ArgoUrlOption : Option<string>
{
    public ArgoUrlOption() : base("--argocd-url")
    {
        Description = "ArgoCD API URL (e.g. https://argo-url)";
    }   
}