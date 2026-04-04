using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Talaryon.StackManager;

public class LocalConfig
{
    private static readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".stackmgr");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "local.json");
    private static readonly string KeyDirectoryPath = Path.Combine(DirectoryPath, "keys");

    private static LocalConfig? _config;
    private static IDataProtector? _protector;
    
    static LocalConfig()
    {
        if(!Directory.Exists(DirectoryPath)) 
            Directory.CreateDirectory(DirectoryPath);
        
        if(!Directory.Exists(KeyDirectoryPath))
            Directory.CreateDirectory(KeyDirectoryPath);
        
        // Initialize data protection provider
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDataProtection()
            .SetApplicationName("Talaryon.StackManager")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
            .PersistKeysToFileSystem(new DirectoryInfo(KeyDirectoryPath));
        
        var services = serviceCollection.BuildServiceProvider();
        var dataProtectionProvider = services.GetDataProtectionProvider();
        _protector = dataProtectionProvider.CreateProtector("Talaryon.StackManager.LocalConfig.v1");
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
        // Create a copy of remotes with encrypted tokens
        var remotesForSerialization = Remotes.Select(r => new 
        {
            name = r.Name,
            url = r.Url,
            access_token = Encrypt(r.AccessToken) // Manually encrypt the token
        }).ToList();
        
        // Create anonymous object for serialization
        var configForSerialization = new 
        {
            app_repository = AppRepository,
            remotes = remotesForSerialization
        };
        
        File.WriteAllText(FilePath, JsonSerializer.Serialize(configForSerialization, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
    
    public static string Encrypt(string data)
    {
        if (string.IsNullOrEmpty(data) || _protector is null)
            return data;

        var protectedData = _protector.Protect(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(protectedData);
    }
    
    public static string Decrypt(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData) || _protector is null)
            return encryptedData;

        try
        {
            var protectedData = Convert.FromBase64String(encryptedData);
            var unprotectedData = _protector.Unprotect(protectedData);
            return Encoding.UTF8.GetString(unprotectedData);
        }
        catch
        {
            return encryptedData; // Fallback for already encrypted data
        }
    }
    
    [JsonPropertyName("app_repository")] public string AppRepository { get; set; } = "";
    [JsonPropertyName("remotes")] public List<LocalConfigRemote> Remotes { get; init; } = [];
    [JsonPropertyName("default")] public LocalConfigDefault Default { get; init; } = new();
    [JsonPropertyName("debugMode")] public bool DebugMode { get; set; }
}

public class LocalConfigDefault
{
    [JsonPropertyName("environment")] public string? Environment { get; set; }
    [JsonPropertyName("stack")] public string? Stack { get; set; }
}

public class LocalConfigRemote
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("url")] public required string Url { get; init; }
    
    [JsonPropertyName("access_token")]
    public string EncryptedAccessToken { get; set; } = "";
    
    [JsonIgnore]
    public string AccessToken
    {
        get => LocalConfig.Decrypt(EncryptedAccessToken);
        set => EncryptedAccessToken = LocalConfig.Encrypt(value);
    }
}