using System.Net.Http.Json;
using System.Text.Json;
using stackmgr.Options;

namespace stackmgr;

public class RKE2
{
    private static readonly StackMgrConfig? Config;

    static RKE2()
    {
        Config = StackMgrConfig.Load();
    }


    public static async Task CreateNamespace(StackEnvironment env, string name)
    {
        var envConfig = Config.Environments.First(v => v.Name.Equals(env.ToString(), StringComparison.CurrentCultureIgnoreCase));
        
        var request = new Dictionary<string, string?>
        {
            { "containerDefaultResourceLimit", null },
            { "name", name },
            { "projectId", envConfig.RKE2.ProjectId },
            { "resourceQuota", null }
        };
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {envConfig.RKE2.AccessToken}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        var response = await client.PostAsJsonAsync($"{envConfig.RKE2.Url}/cluster/local/namespaces", request);
        
        // var res = await response.Content.ReadFromJsonAsync<object>();
        var res = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine(res);
    }

    public static async Task DeleteNamespace(StackEnvironment env, string name)
    {
        
    }
    
}