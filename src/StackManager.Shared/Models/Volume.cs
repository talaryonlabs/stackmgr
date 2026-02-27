using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("volumes", ApiEndpointType.Create)]
[ApiEndpoint("volumes/{name}", ApiEndpointType.Single | ApiEndpointType.Delete)]
public class Volume
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("size")] public required long Size { get; init; }
    [JsonPropertyName("accessMode")] public required string AccessMode { get; init; }
    [JsonPropertyName("numberOfReplicas")] public int NumberOfReplicas { get; init; } = 2;
    [JsonPropertyName("state")] public string? State { get; init; }
    [JsonPropertyName("frontend")] public string Frontend { get; init; } = "blockdev";
    [JsonPropertyName("labels")] public Dictionary<string, string> Labels { get; init; } = [];
    [JsonPropertyName("reuseVolume")] public bool ReuseVolume { get; init; }
}

[ApiEndpoint("volumes")]
public class VolumeList : List<Volume>;