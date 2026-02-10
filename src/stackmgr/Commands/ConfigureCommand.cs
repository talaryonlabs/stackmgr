using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using Talaryon.Toolbox.Extensions;

namespace stackmgr.Commands;

public class ConfigureCommand : StackManagerCommand
{
    public ConfigureCommand() : base("configure", "Configure a resource (environment, stack)")
    {
        var env = new StackManagerCommand("environment", "Configure a stack environment")
        {
            new EnvironmentArgument(),
            new RancherAccessTokenOption(),
            new RancherUrlOption(),
            new RancherProjectIdOption(),
            new ArgoUrlOption(),
            new ArgoAccessTokenOption(),
            new ArgoProjectOption(),
            new ArgoRepositoryOption(),
        };
        env.Aliases.Add("env");
        env.SetAction(ConfigureEnvironment);

        var stack = new StackManagerCommand("stack", "Configure a stack")
        {
            new EnvironmentOption { Required = true },
            new StackArgument(),
            new AutoSyncOption(),
            new VaultOption()
        };
        stack.Aliases.Add("s");
        stack.SetAction(ConfigureStack);
        
        Add(env);
        Add(stack);
    }

    private void ConfigureEnvironment(ParseResult v)
    {
        var env = GetEnvironment<EnvironmentArgument>(v);
            
        var rke2AccessToken = v.GetValue<string, RancherAccessTokenOption>();
        if (rke2AccessToken is not null)
        {
            env.RKE2.AccessToken = rke2AccessToken.ToBase64String();
            Console.WriteLine($"RKE2 access token updated for environment '{env.Name}'.");
        }
        
        var rke2Url = v.GetValue<string, RancherUrlOption>();
        if (rke2Url is not null)
        {
            env.RKE2.Url = rke2Url;
            Console.WriteLine("RKE2 URL updated.");
        }
        
        var rke2ProjectId = v.GetValue<string, RancherProjectIdOption>();
        if (rke2ProjectId is not null)
        {
            env.RKE2.ProjectId = rke2ProjectId;
            Console.WriteLine("RKE2 project ID updated.");
        }
        
        var argoUrl = v.GetValue<string, ArgoUrlOption>();
        if (argoUrl is not null)
        {
            env.ArgoCD.Url = argoUrl;
            Console.WriteLine("ArgoCD URL updated.");
        }
        
        var argoAccessToken = v.GetValue<string, ArgoAccessTokenOption>();
        if (argoAccessToken is not null)
        {
            env.ArgoCD.AccessToken = argoAccessToken.ToBase64String();
            Console.WriteLine("ArgoCD access token updated.");
        }
        
        var argoProject = v.GetValue<string, ArgoProjectOption>();
        if (argoProject is not null)
        {
            env.ArgoCD.Project = argoProject;
            Console.WriteLine("ArgoCD project updated.");
        }
        
        var argoRepository = v.GetValue<string, ArgoRepositoryOption>();
        if (argoRepository is not null)
        {
            env.ArgoCD.Repository = argoRepository;
            Console.WriteLine("ArgoCD repository updated.");
        }
        
        Config.Save();
    }
    
    private void ConfigureStack(ParseResult v)
    {
        var env = GetEnvironment<EnvironmentOption>(v);
        var stack = GetStack<StackArgument>(v, env);
        
        var vault = v.GetValue<string, VaultOption>();
        if (!string.IsNullOrEmpty(vault))
        {
            stack.Vault = vault;
        }
        
        stack.EnableAutoSync = v.GetValue<bool, AutoSyncOption>(); // TODO: will always be false if not set
        stack.SaveConfig();
    }
}