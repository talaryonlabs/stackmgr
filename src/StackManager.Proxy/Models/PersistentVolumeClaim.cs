using System.Text.Json.Serialization;

namespace Talaryon.StackManager.Proxy.Models;

public class PersistentVolumeClaim
{
    [JsonPropertyName("id")] public string Id => $"{Namespace}/{Name}"; // Format: "{namespace}/{name}"
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("volumeName")] public required string VolumeName { get; init; }
    [JsonPropertyName("storageSize")] public required string StorageSize { get; init; }
    [JsonPropertyName("accessMode")] public required string AccessMode { get; init; }
    [JsonPropertyName("namespace")] public string? Namespace { get; set; }
    [JsonPropertyName("status")] public string? Status { get; init; } // "Bound", "Pending", etc.
}