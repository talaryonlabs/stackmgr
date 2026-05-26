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

    protected override void Configure()
    {
        var env = GetEnvironment<EnvironmentArgument>();
        
        var repository = GetValue<string, RepositoryOption>();
        if (!string.IsNullOrEmpty(repository))
        {
            env.Repository = repository;
            LogMessage.AsSuccess($"Repository '{repository}' configured for environment '{env.Name}'.");
        }
        
        var vault = GetValue<string, VaultOption>();
        if (!string.IsNullOrEmpty(vault))
        {
            env.Vault = vault;
            LogMessage.AsSuccess($"Vault '{vault}' configured for environment '{env.Name}'.");
        }
        
        var registryCredentials = GetValue<string, RegistryCredentialsOption>();
        if (!string.IsNullOrEmpty(registryCredentials))
        {
            env.RegistryCredentials = registryCredentials;
            LogMessage.AsSuccess($"Registry credentials configured for environment '{env.Name}'.");
        }
        
        var outpost = GetValue<string, OutpostOption>();
        if (!string.IsNullOrEmpty(outpost))
        {
            env.Outpost = outpost;
            LogMessage.AsSuccess($"Outpost '{outpost}' configured for environment '{env.Name}'.");
        }
        
        var certIssuer = GetValue<string, CertIssuerOption>();
        if (!string.IsNullOrEmpty(certIssuer))
        {
            env.CertIssuer = certIssuer;
            LogMessage.AsSuccess($"CertIssuer '{certIssuer}' configured for environment '{env.Name}'.");
        }
        
        var remote = GetValue<string, RemoteOption>();
        if (!string.IsNullOrEmpty(remote))
        {
            env.Remote = remote;
            LogMessage.AsSuccess($"Remote '{remote}' configured for environment '{env.Name}'.");
        }
        
        env.Save();
    }
}
