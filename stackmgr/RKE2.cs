using System.Net.Http.Json;
using System.Text.Json;

namespace stackmgr;

public class RKE2
{
    private static readonly StackMgrConfig? Config;

    static RKE2()
    {
        Config = StackMgrConfig.Load();
    }


    public static async Task CreateNamespace(string name)
    {
        var request = new Dictionary<string, string?>
        {
            { "containerDefaultResourceLimit", null },
            { "name", name },
            { "projectId", "" },
            { "resourceQuota", null }
        };
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.RKE2.AccessToken}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        var response = await client.PostAsJsonAsync($"{Config.RKE2.Url}/cluster/local/namespaces", request);
        
        // var res = await response.Content.ReadFromJsonAsync<object>();
        var res = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine(res);
    }
}