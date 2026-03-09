using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Talaryon.Toolbox;

namespace Talaryon.StackManager.Services;

public class GenService : IDisposable, ITalaryonRunner<string>
{
    private readonly HttpClient _client;
    private readonly Dictionary<string, string> _files = new();
    
    private GenServiceInfo? _info;

    public GenService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://raw.githubusercontent.com/talaryonlabs/stackmgr/refs/heads/main/deployment/");
        _client.DefaultRequestHeaders.Add("Accept", [
            "application/json",
            "application/x-www-form-urlencoded",
            "application/yaml"
        ]);
    }

    public async Task GetGenFileAsync()
    {
        var response = await _client.GetAsync("gen.json", CancellationToken.None);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to request gen.json. Response code: {response.StatusCode}");
        }
        _info = await response.Content.ReadFromJsonAsync<GenServiceInfo>();
    }

    public async Task GetGenFilesAsync()
    {
        if (_files.Count > 0)
        {
            throw new Exception("Files already requested");
        }
        
        var tasks = _info.Files.Select(v => _client.GetStringAsync(v, CancellationToken.None));
        
        await Task.WhenAll(tasks.ToArray());
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public Task<string> RunAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}

public class GenServiceInfo
{
    [JsonPropertyName("files")] public IEnumerable<string> Files { get; set; } = [];
    [JsonPropertyName("params")] public IEnumerable<GenServiceInfoParam> Params { get; set; } = [];   
}

public class GenServiceInfoParam
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("default")] public required string Default { get; set; }   
}