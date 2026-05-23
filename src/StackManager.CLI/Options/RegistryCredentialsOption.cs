using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RegistryCredentialsOption : Option<string>
{
    public RegistryCredentialsOption() : base("--registry-credentials")
    {
        Description = "Vault path to container registry credentials (e.g. kv-stack/data/ghcr.io)";
    }
}