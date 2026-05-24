using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Environments;

/// <summary>
/// Command for configuring an environment.
/// </summary>
public class ConfigureEnvironmentCommand : ResourceConfigureCommand<EnvironmentArgument>
{
    public ConfigureEnvironmentCommand()
        : base("environment", "Configure a stack environment")
    {
        Add(new AppRepositoryOption());
        Add(new VaultOption());
        Add(new OutpostOption());
        Add(new CertIssuerOption());
        Add(new RegistryCredentialsOption());
        Add(new RemoteOption());
        Add(new RepositoryOption());
    }

    protected override void Configure(ParseResult parseResult)
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
}
