using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("namespaces", ApiEndpointType.Create)]
[ApiEndpoint("namespaces/{name}", ApiEndpointType.Single | ApiEndpointType.Delete)]
public class Namespace : IApiResource
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("project")] public string? Project { get; set; }
}

[ApiEndpoint("namespaces")]
public class NamespaceList : List<Namespace>;