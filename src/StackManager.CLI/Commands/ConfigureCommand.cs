using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager.Commands;

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
            new OutpostOption(),
            new CertIssuerOption(),
            new RegistryCredentialsOption(),
            new RemoteOption()
        };
        env.Aliases.Add("env");
        env.SetAction(ConfigureEnvironment);

        var stack = new StackManagerCommand("stack", "Configure a stack")
        {
            new EnvironmentOption { Required = true },
            new StackArgument(),
            new EnableAutoSyncOption(),

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
            LogMessage.AsSuccess("App repository updated.");
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
            LogMessage.AsSuccess($"RKE2 access token updated for environment '{env.Name}'.");
        }
        
        var argoAccessToken = parseResult.GetValue<string, ArgoAccessTokenOption>();
        if (argoAccessToken is not null)
        {
            env.Argo.SetAccessToken(env,
                argoAccessToken.StartsWith("base64:") ? argoAccessToken[7..] : argoAccessToken.ToBase64String());
            
            LogMessage.AsSuccess("ArgoCD access token updated.");
        }
        
        var rke2Url = parseResult.GetValue<string, RancherUrlOption>();
        if (rke2Url is not null)
        {
            env.Rancher.Url = rke2Url;
            LogMessage.AsSuccess("RKE2 URL updated.");
        }
            
        var rke2ProjectId = parseResult.GetValue<string, RancherProjectIdOption>();
        if (rke2ProjectId is not null)
        {
            env.Rancher.ProjectId = rke2ProjectId;
            LogMessage.AsSuccess("RKE2 project ID updated.");
        }
            
        var argoUrl = parseResult.GetValue<string, ArgoUrlOption>();
        if (argoUrl is not null)
        {
            env.Argo.Url = argoUrl;
            LogMessage.AsSuccess("ArgoCD URL updated.");
        }

        var argoProject = parseResult.GetValue<string, ArgoProjectOption>();
        if (argoProject is not null)
        {
            env.Argo.Project = argoProject;
            LogMessage.AsSuccess("ArgoCD project updated.");
        }
            
        var argoRepository = parseResult.GetValue<string, ArgoRepositoryOption>();
        if (argoRepository is not null)
        {
            env.Argo.Repository = argoRepository;
            LogMessage.AsSuccess("ArgoCD repository updated.");
        }
        
        
        var vault = parseResult.GetValue<string, VaultOption>();
        if (!string.IsNullOrEmpty(vault))
        {
            env.Vault = vault;
            LogMessage.AsSuccess($"Vault '{vault}' configured for environment '{env.Name}'.");
        }
        
        var registryCredentials = parseResult.GetValue<string, RegistryCredentialsOption>();
        if (!string.IsNullOrEmpty(registryCredentials))
        {
            env.RegistryCredentials = registryCredentials;
            LogMessage.AsSuccess($"Registry credentials configured for environment '{env.Name}'.");
        }
        
        var outpost = parseResult.GetValue<string, OutpostOption>();
        if (!string.IsNullOrEmpty(outpost))
        {
            env.Outpost = outpost;
            LogMessage.AsSuccess($"Outpost '{outpost}' configured for environment '{env.Name}'.");
        }
        
        var certIssuer = parseResult.GetValue<string, CertIssuerOption>();
        if (!string.IsNullOrEmpty(certIssuer))
        {
            env.CertIssuer = certIssuer;
            LogMessage.AsSuccess($"CertIssuer '{certIssuer}' configured for environment '{env.Name}'.");
        }
        
        var remote = parseResult.GetValue<string, RemoteOption>();
        if (!string.IsNullOrEmpty(remote))
        {
            env.Remote = remote;
            LogMessage.AsSuccess($"Remote '{remote}' configured for environment '{env.Name}'.");
        }
        
        env.SaveConfig();
    }
    
    private void ConfigureStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);

        if (parseResult.Tokens.Any(v => v.Value == "--enable-auto-sync"))
        {
            stack.EnableAutoSync = parseResult.GetValue<bool, EnableAutoSyncOption>();
        }
        
        stack.SaveConfig();
    }
}