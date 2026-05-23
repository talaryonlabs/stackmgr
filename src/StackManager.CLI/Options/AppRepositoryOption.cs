using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class AppRepositoryOption : Option<string>
{
    public AppRepositoryOption() : base("--app-repository")
    {
        Description = "The repository url for application manifests";
    }
}