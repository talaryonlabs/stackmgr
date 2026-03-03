using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RepositoryOption : Option<string>
{
    public RepositoryOption() : base("--repository")
    {
        Description = "repository name";
    }   
}