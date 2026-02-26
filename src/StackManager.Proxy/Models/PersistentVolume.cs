using System.Text.Json.Serialization;

namespace Talaryon.StackManager.Proxy.Models;

public class PersistentVolume
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("storageSize")] public required string StorageSize { get; init; }
    [JsonPropertyName("accessMode")] public required string AccessMode { get; init; }
    [JsonPropertyName("volumeHandle")] public required string VolumeHandle { get; init; } // Longhorn-Volume-Name
//     public required string Status { get; init; } // "Available", "Bound", etc.
//     public required string PersistentVolumeReclaimPolicy { get; init; } = "Retain";
//     public required string VolumeMode { get; init; } // "Filesystem", "Block"
}