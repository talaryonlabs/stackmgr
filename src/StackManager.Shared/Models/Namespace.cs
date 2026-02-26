using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("namespaces", ApiEndpointType.Many | ApiEndpointType.Create)]
[ApiEndpoint("namespaces/{name}", ApiEndpointType.Single | ApiEndpointType.Delete)]
public class Namespace
{
    public required string Name { get; set; }
    public string? Project { get; set; }
}