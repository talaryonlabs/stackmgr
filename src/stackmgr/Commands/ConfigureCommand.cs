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
            new AppRepositoryOption()
        };
        env.Aliases.Add("env");
        env.SetAction(ConfigureEnvironment);

        var stack = new StackManagerCommand("stack", "Configure a stack")
        {
            new EnvironmentOption { Required = true },
            new StackArgument(),
            new AutoSyncOption(),
            new VaultOption(),
            new RegistryCredentialsOption()
        };
        stack.Aliases.Add("s");
        stack.SetAction(ConfigureStack);
        
        Add(env);
        Add(stack);
    }

    private void ConfigureEnvironment(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentArgument>(parseResult);
            
        var rke2AccessToken = parseResult.GetValue<string, RancherAccessTokenOption>();
        if (rke2AccessToken is not null)
        {
            env.Rancher.AccessToken = rke2AccessToken.ToBase64String();
            HelperMethods.LogSuccess($"RKE2 access token updated for environment '{env.Name}'.");
        }
        
        var rke2Url = parseResult.GetValue<string, RancherUrlOption>();
        if (rke2Url is not null)
        {
            env.Rancher.Url = rke2Url;
            HelperMethods.LogSuccess("RKE2 URL updated.");
        }
        
        var rke2ProjectId = parseResult.GetValue<string, RancherProjectIdOption>();
        if (rke2ProjectId is not null)
        {
            env.Rancher.ProjectId = rke2ProjectId;
            HelperMethods.LogSuccess("RKE2 project ID updated.");
        }
        
        var argoUrl = parseResult.GetValue<string, ArgoUrlOption>();
        if (argoUrl is not null)
        {
            env.Argo.Url = argoUrl;
            HelperMethods.LogSuccess("ArgoCD URL updated.");
        }
        
        var argoAccessToken = parseResult.GetValue<string, ArgoAccessTokenOption>();
        if (argoAccessToken is not null)
        {
            env.Argo.AccessToken = argoAccessToken.ToBase64String();
            HelperMethods.LogSuccess("ArgoCD access token updated.");
        }
        
        var argoProject = parseResult.GetValue<string, ArgoProjectOption>();
        if (argoProject is not null)
        {
            env.Argo.Project = argoProject;
            HelperMethods.LogSuccess("ArgoCD project updated.");
        }
        
        var argoRepository = parseResult.GetValue<string, ArgoRepositoryOption>();
        if (argoRepository is not null)
        {
            env.Argo.Repository = argoRepository;
            HelperMethods.LogSuccess("ArgoCD repository updated.");
        }
        
        var appRepository = parseResult.GetValue<string, AppRepositoryOption>();
        if (appRepository is not null)
        {
            env.AppRepository = appRepository;
            HelperMethods.LogSuccess("App repository updated.");
        }
        
        Config.Save();
    }
    
    private void ConfigureStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);
        
        var vault = parseResult.GetValue<string, VaultOption>();
        if (!string.IsNullOrEmpty(vault))
        {
            stack.Vault = vault;
            HelperMethods.LogSuccess($"Vault '{vault}' configured for stack '{stack.Name}'.");
        }
        
        var registryCredentials = parseResult.GetValue<string, RegistryCredentialsOption>();
        if (!string.IsNullOrEmpty(registryCredentials))
        {
            stack.RegistryCredentials = registryCredentials;
            HelperMethods.LogSuccess($"Registry credentials configured for stack '{stack.Name}'.");
        }
        
        stack.EnableAutoSync = parseResult.GetValue<bool, AutoSyncOption>(); // TODO: will always be false if not set
        stack.SaveConfig();
    }
}