using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class WithoutIngressOption : Option<bool>
{
    public WithoutIngressOption() : base("--without-ingress")
    {
        Description = "Copy template without ingress.";
    }
}