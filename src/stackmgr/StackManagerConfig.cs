using System.Text.Json;
using System.Text.Json.Serialization;

namespace stackmgr;

public class StackManagerConfig
{
    private static readonly string FilePath = Path.Combine(Environment.CurrentDirectory, ".stackmgr");
    private static StackManagerConfig? _config;

    public static StackManagerConfig Load()
    {
        if (File.Exists(FilePath) && _config is null)
        {
            var content = File.ReadAllText(FilePath);
            _config = JsonSerializer.Deserialize<StackManagerConfig>(content);
        }
        return _config ?? new StackManagerConfig();
    }

    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
    
    [JsonPropertyName("environments")] public List<StackEnvironment> Environments { get; set; } = [];
}