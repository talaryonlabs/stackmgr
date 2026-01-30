using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Services;

public class ArgoCDApplication
{
    public ArgoCDApplicationMetadata Metadata { get; set; }
    public ArgoCDApplicationSpec Spec { get; set; }

    public static implicit operator ArgoCDApplication(V1alpha1Application application) => new()
    {
        Metadata = new()
        {
            Name = application.Metadata.Name
        },
        Spec = new()
        {
            SyncPolicy = application.Spec.SyncPolicy,
            Source = application.Spec.Source,
            Destination = application.Spec.Destination
        }
    };
}

public class ArgoCDApplicationMetadata
{
    public string Name { get; set; }
}

public class ArgoCDApplicationSpec
{
    public V1alpha1SyncPolicy SyncPolicy { get; set; }
    public V1alpha1ApplicationSource Source { get; set; }
    public V1alpha1ApplicationDestination Destination { get; set; }
}