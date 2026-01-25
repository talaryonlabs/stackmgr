using System.Text.Json.Serialization;

namespace stackmgr;

public class StackEnvironment
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("rke2")] public StackEnvironmentRKE2 RKE2 { get; set; } = new();
    [JsonPropertyName("argocd")] public StackEnvironmentArgoCD ArgoCD { get; set; } = new();
}

public class StackEnvironmentRKE2
{
    [JsonPropertyName("projectId")] public string ProjectId { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
}

public class StackEnvironmentArgoCD
{
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
}