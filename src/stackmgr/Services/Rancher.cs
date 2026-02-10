using System.Net;
using System.Net.Http.Json;
using Talaryon.Toolbox.Extensions;

namespace stackmgr.Services;

public class Rancher
{
    private static HttpClient NewClient(StackEnvironment env)
    {
        var rke2Client = new HttpClient();
        rke2Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {env.RKE2.AccessToken.FromBase64String()}");
        rke2Client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        return rke2Client;
    }
    

    public static bool CheckRequirements(StackEnvironment env)
    {
        if (env.RKE2.AccessToken is null or "")
        {
            Console.WriteLine("No access token provided. Please check your configuration.");
            return false;
        }
        if (env.RKE2.Url is null or "")
        {
            Console.WriteLine("No RKE2 URL provided. Please check your configuration.");
            return false;
        }
        if (env.RKE2.ProjectId is null or "")
        {
            Console.WriteLine("No RKE2 project ID provided. Please check your configuration.");
            return false;
        }
        return true;
    }

    public static async Task TestConnection(StackEnvironment env)
    {
        if (!CheckRequirements(env)) return;
        
        Console.WriteLine($" - API URL: {env.RKE2.Url}");
        Console.WriteLine($" - Project ID: {env.RKE2.ProjectId}");
            
        using var client = NewClient(env);
        var response = await client.GetAsync($"{env.RKE2.Url}/v3/projects/{env.RKE2.ProjectId}");
            
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                Console.WriteLine("Unauthorized. Please check your access token.");
                return;
            case HttpStatusCode.NotFound:
                Console.WriteLine("Project not found. Please check your project ID.");
                return;
            default:
                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Success."
                    : $"Failed. Response code: {response.StatusCode}");
                break;
        }
    }

    public static async Task<bool> NamespaceExists(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return false;
        
        using var client = NewClient(env);
        var response = await client.GetAsync($"{env.RKE2.Url}/v3/cluster/local/namespaces/{name}");
        
        return response.IsSuccessStatusCode;
    }


    public static async Task<bool> CreateNamespace(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return false;
        
        var request = new Dictionary<string, string?>
        {
            { "containerDefaultResourceLimit", null },
            { "name", name },
            { "projectId", env.RKE2.ProjectId },
            { "resourceQuota", null }
        };
        
        using var client = NewClient(env);
        var response = await client.PostAsJsonAsync($"{env.RKE2.Url}/v3/cluster/local/namespaces", request);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to create namespace '{name}'. Response code: {response.StatusCode}");
            return false;
        }

        return true;
    }

    public static async Task<bool> DeleteNamespace(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return false;
        
        using var client = NewClient(env);
        var response = await client.DeleteAsync($"{env.RKE2.Url}/v3/cluster/local/namespaces/{name}");
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to delete namespace '{name}'. Response code: {response.StatusCode}");
            return false;
        }

        return true;
    }
    
}