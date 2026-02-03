using System.Text.Json;
using System.Text.Json.Serialization;

namespace stackmgr;

public class StackManagerConfig
{
    private const string FileName = ".stackmgr";
    
    private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

    public static bool Exists => new FileInfo(FilePath).Exists;
    
    public static StackManagerConfig Load() => !Exists ? new() : JsonSerializer.Deserialize<StackManagerConfig>(File.ReadAllText(FilePath)) ?? new();

    [JsonPropertyName("environments")] public List<StackEnvironment> Environments { get; set; } = [];


    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}