using System.CommandLine;

namespace stackmgr.Options;

public class ArgoCDUrlOption : Option<string>
{
    public ArgoCDUrlOption() : base("--argocd-url")
    {
        Description = "ArgoCD API URL (e.g. https://argo-url)";
    }   
}