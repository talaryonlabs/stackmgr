using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("applications", ApiEndpointType.Create)]
[ApiEndpoint("applications/{name}", ApiEndpointType.Single | ApiEndpointType.Delete | ApiEndpointType.Update)]
public class Application
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("repository")] public required string Repository { get; set; }
    [JsonPropertyName("path")] public required string Path { get; set; }
    [JsonPropertyName("project")] public string? Project { get; set; }
    [JsonPropertyName("autoSyncEnabled")] public bool IsAutoSyncEnabled { get; set; }
}

[ApiEndpoint("applications")]
public class ApplicationList : List<Application>;