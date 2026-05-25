namespace Talaryon.StackManager.Models.Kubernetes;

public class OutpostService : Service
{
    public const string FileName = "svc.outpost.yaml";
    
    public OutpostService(Stack stack)
    {
        Metadata.Name = $"{stack.Name}-auth";
        Spec.Type = "ExternalName";
        Spec.ExternalName = stack.Environment.Outpost;
    }
}