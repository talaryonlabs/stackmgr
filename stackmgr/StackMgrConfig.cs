using System.Text.Json;
using System.Text.Json.Serialization;

namespace stackmgr;

public class StackMgrConfig
{
    public const string FileName = ".stackmgr";
    
    private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

    public static bool Exists => new FileInfo(FilePath).Exists;
    
    public static void Create()
    {
        var json = JsonSerializer.Serialize(new StackMgrConfig(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
    
    public static StackMgrConfig? Load() => !Exists ? null : JsonSerializer.Deserialize<StackMgrConfig>(File.ReadAllText(FilePath));

    [JsonPropertyName("rke2")] public StackMgrService RKE2 { get; set; } = new();
    [JsonPropertyName("argocd")] public StackMgrService ArgoCD { get; set; } = new();
}

public class StackMgrService
{
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
}