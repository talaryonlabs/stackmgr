using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("volumes/{namespace}", ApiEndpointType.Create)]
[ApiEndpoint("volumes/{namespace}/{name}", ApiEndpointType.Single | ApiEndpointType.Delete)]
public class Volume : IApiResource
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("size")] public required string Size { get; init; }
    [JsonPropertyName("accessMode")] public required string AccessMode { get; init; }
    [JsonPropertyName("numberOfReplicas")] public int NumberOfReplicas { get; init; } = 2;
    [JsonPropertyName("state")] public string? State { get; init; }
    [JsonPropertyName("frontend")] public string Frontend { get; init; } = "blockdev";
    [JsonPropertyName("labels")] public Dictionary<string, string> Labels { get; init; } = [];
}

[ApiEndpoint("volumes/{namespace}")]
public class VolumeList : List<Volume>;