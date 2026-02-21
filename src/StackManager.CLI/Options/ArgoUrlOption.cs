using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ArgoUrlOption : Option<string>
{
    public ArgoUrlOption() : base("--argocd-url")
    {
        Description = "ArgoCD API URL (e.g. https://argo-url)";
    }   
}