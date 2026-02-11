using System.Text.Json.Serialization;

namespace stackmgr;

public class StackEnvironment : IStackManagerEntity
{
    [JsonIgnore] public DirectoryInfo LocalDirectory => new(Path.Combine(Environment.CurrentDirectory, Name));
    
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("appRepository")] public string AppRepository { get; set; } = "";
    [JsonPropertyName("rke2")] public StackEnvironmentRancher Rancher { get; set; } = new();
    [JsonPropertyName("argocd")] public StackEnvironmentArgo Argo { get; set; } = new();
}

public class StackEnvironmentRancher
{
    [JsonPropertyName("projectId")] public string ProjectId { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
}

public class StackEnvironmentArgo
{
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("project")] public string Project { get; set; } = "";
    [JsonPropertyName("repository")] public string Repository { get; set; } = "";
}