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
            new AppRepositoryOption(),
            new VaultOption(),
            new OutpostOption(),
            new CertIssuerOption(),
            new RegistryCredentialsOption(),
            new RemoteOption(),
            new RepositoryOption()
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
        
        var repository = parseResult.GetValue<string, RepositoryOption>();
        if (!string.IsNullOrEmpty(repository))
        {
            env.Repository = repository;
            LogMessage.AsSuccess($"Repository '{repository}' configured for environment '{env.Name}'.");
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