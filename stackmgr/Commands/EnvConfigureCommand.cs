using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using Talaryon.Toolbox.Extensions;

namespace stackmgr.Commands;

public class EnvConfigureCommand : Command
{
    public EnvConfigureCommand() : base("configure", "Configure environment variables")
    {
        Add(new EnvironmentArgument());
        Add(new RKE2AccessTokenOption());
        Add(new RKE2UrlOption());
        Add(new RKE2ProjectIdOption());
        Add(new ArgoCDServiceOption());
        Add(new ArgoCDNamespaceOption());
        SetAction(v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            var rke2AccessToken = v.GetValue<string, RKE2AccessTokenOption>();
            if (rke2AccessToken is not null)
            {
                env.RKE2.AccessToken = rke2AccessToken.ToBase64String();
                Console.WriteLine("RKE2 access token updated.");
            }
            
            var rke2Url = v.GetValue<string, RKE2UrlOption>();
            if (rke2Url is not null)
            {
                env.RKE2.Url = rke2Url;
                Console.WriteLine("RKE2 URL updated.");
            }
            
            var rke2ProjectId = v.GetValue<string, RKE2ProjectIdOption>();
            if (rke2ProjectId is not null)
            {
                env.RKE2.ProjectId = rke2ProjectId;
                Console.WriteLine("RKE2 project ID updated.");
            }
            
            var argoService = v.GetValue<string, ArgoCDServiceOption>();
            if (argoService is not null)
            {
                env.ArgoCD.Service = argoService;
                Console.WriteLine("ArgoCD service updated.");
            }
            
            var argoNamespace = v.GetValue<string, ArgoCDNamespaceOption>();
            if (argoNamespace is not null)
            {
                env.ArgoCD.Namespace = argoNamespace;
                Console.WriteLine("ArgoCD namespace updated.");
            }
            
            config.Save();
        });
    }
}