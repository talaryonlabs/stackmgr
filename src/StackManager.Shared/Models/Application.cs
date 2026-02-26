using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("applications", ApiEndpointType.Many | ApiEndpointType.Create)]
[ApiEndpoint("applications/{name}", ApiEndpointType.Single | ApiEndpointType.Delete | ApiEndpointType.Update)]
public class Application
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("project")] public string? Project { get; set; }
    [JsonPropertyName("repository")] public string? Repository { get; set; }
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("targetRevision")] public string? TargetRevision { get; set; }
    [JsonPropertyName("autoSyncEnabled")] public bool IsAutoSyncEnabled { get; set; }
}