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
            new AppRepositoryOption(),
            new VaultOption(),
            new RegistryCredentialsOption()
        };
        env.Aliases.Add("env");
        env.SetAction(ConfigureEnvironment);

        var stack = new StackManagerCommand("stack", "Configure a stack")
        {
            new EnvironmentOption { Required = true },
            new StackArgument(),
            new AutoSyncOption(),

        };
        stack.Aliases.Add("s");
        stack.SetAction(ConfigureStack);

        var global = new StackManagerCommand("global", "Configure the app repository")
        {
            new AppRepositoryOption()
        };
        global.Aliases.Add("g");
        global.SetAction(ConfigureGlobal);
        
        Add(env);
        Add(stack);
        Add(global);
    }

    private void ConfigureGlobal(ParseResult parseResult)
    {
        var localConfig = LocalConfig.Get();
        
        var appRepository = parseResult.GetValue<string, AppRepositoryOption>();
        if (appRepository is not null)
        {
            localConfig.AppRepository = appRepository;
            HelperMethods.LogSuccess("App repository updated.");
        }
        
        localConfig.Save();
    }

    private void ConfigureEnvironment(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentArgument>(parseResult);

        var rke2AccessToken = parseResult.GetValue<string, RancherAccessTokenOption>();
        if (rke2AccessToken is not null)
        {
            env.Rancher.SetAccessToken(env,
                rke2AccessToken.StartsWith("base64:") ? rke2AccessToken[7..] : rke2AccessToken.ToBase64String());
            HelperMethods.LogSuccess($"RKE2 access token updated for environment '{env.Name}'.");
        }
        
        var argoAccessToken = parseResult.GetValue<string, ArgoAccessTokenOption>();
        if (argoAccessToken is not null)
        {
            env.Argo.SetAccessToken(env,
                argoAccessToken.StartsWith("base64:") ? argoAccessToken[7..] : argoAccessToken.ToBase64String());
            
            HelperMethods.LogSuccess("ArgoCD access token updated.");
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
        
        
        var vault = parseResult.GetValue<string, VaultOption>();
        if (!string.IsNullOrEmpty(vault))
        {
            env.Vault = vault;
            HelperMethods.LogSuccess($"Vault '{vault}' configured for environment '{env.Name}'.");
        }
        
        var registryCredentials = parseResult.GetValue<string, RegistryCredentialsOption>();
        if (!string.IsNullOrEmpty(registryCredentials))
        {
            env.RegistryCredentials = registryCredentials;
            HelperMethods.LogSuccess($"Registry credentials configured for environment '{env.Name}'.");
        }
        
        env.SaveConfig();
    }
    
    private void ConfigureStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);
        
        stack.EnableAutoSync = parseResult.GetValue<bool, AutoSyncOption>(); // TODO: will always be false if not set
        stack.SaveConfig();
    }
}