using System.CommandLine;

namespace stackmgr.Options;

public class AppRepositoryOption : Option<string>
{
    public AppRepositoryOption() : base("--app-repository")
    {
        Description = "The repository url for application manifests";
    }
}