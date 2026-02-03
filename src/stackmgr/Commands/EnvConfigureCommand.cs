using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using Talaryon.Toolbox.Extensions;

namespace stackmgr.Commands;

public class EnvConfigureCommand : StackManagerCommand
{
    public EnvConfigureCommand() : base("configure", "Configure environment variables")
    {
        Add(new EnvironmentArgument());
        Add(new RKE2AccessTokenOption());
        Add(new RKE2UrlOption());
        Add(new RKE2ProjectIdOption());
        Add(new ArgoCDUrlOption());
        Add(new ArgoCDAccessTokenOption());
        Add(new ArgoCDProjectOption());
        Add(new ArgoCDRepositoryOption());
        SetAction(v =>
        {
            var name = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var env = Config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
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
            
            var argoUrl = v.GetValue<string, ArgoCDUrlOption>();
            if (argoUrl is not null)
            {
                env.ArgoCD.Url = argoUrl;
                Console.WriteLine("ArgoCD URL updated.");
            }
            
            var argoAccessToken = v.GetValue<string, ArgoCDAccessTokenOption>();
            if (argoAccessToken is not null)
            {
                env.ArgoCD.AccessToken = argoAccessToken.ToBase64String();
                Console.WriteLine("ArgoCD access token updated.");
            }
            
            var argoProject = v.GetValue<string, ArgoCDProjectOption>();
            if (argoProject is not null)
            {
                env.ArgoCD.Project = argoProject;
                Console.WriteLine("ArgoCD project updated.");
            }
            
            var argoRepository = v.GetValue<string, ArgoCDRepositoryOption>();
            if (argoRepository is not null)
            {
                env.ArgoCD.Repository = argoRepository;
                Console.WriteLine("ArgoCD repository updated.");
            }
            
            Config.Save();
        });
    }
}