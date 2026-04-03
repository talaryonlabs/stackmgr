using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ReplicasOption : Option<int>
{
    public ReplicasOption() : base("--replicas")
    {
        Description = "number of replicas";
    } 
}