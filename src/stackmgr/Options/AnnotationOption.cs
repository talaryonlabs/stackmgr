using System.CommandLine;

namespace stackmgr.Options;

public class AnnotationOption : Option<string>
{
    public AnnotationOption() : base("--annotation")
    {
        Description = "annotation (e.g. nginx.ingress.kubernetes.io/rewrite-target=hallo)";
        AllowMultipleArgumentsPerToken = true;
    }
}