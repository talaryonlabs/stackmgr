using System.Text.Json;
using System.Text.Json.Serialization;

namespace stackmgr;

public class LocalConfig
{
    private static readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".stackmgr");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "local.json");

    private static LocalConfig? _config;
    
    static LocalConfig()
    {
        if(!Directory.Exists(DirectoryPath)) 
            Directory.CreateDirectory(DirectoryPath);
    }
    
    public static LocalConfig Get()
    {
        if (File.Exists(FilePath) && _config is null)
        {
            var content = File.ReadAllText(FilePath);
            _config = JsonSerializer.Deserialize<LocalConfig>(content);
        }

        return _config ?? new();
    }
    
    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
    
    [JsonPropertyName("app_repository")] public string AppRepository { get; set; } = "";
    [JsonPropertyName("environments")] public List<LocalConfigEnvironment> Environments { get; init; } = [];
}

public class LocalConfigEnvironment
{
    [JsonPropertyName("name")] public required string Name { get; init; } = "";
    [JsonPropertyName("rke2_access_token")] public string RancherAccessToken { get; set; } = "";
    [JsonPropertyName("argo_access_token")] public string ArgoAccessToken { get; set; } = "";
}