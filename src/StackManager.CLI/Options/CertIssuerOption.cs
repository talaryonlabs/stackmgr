using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class CertIssuerOption : Option<string>
{
    public CertIssuerOption() : base("--cert-issuer")
    {
        Description = "issuer name (cert-manager.io/cluster-issuer)";
        Arity = ArgumentArity.ExactlyOne;
    }
}