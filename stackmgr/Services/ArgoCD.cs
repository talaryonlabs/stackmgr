using System.Net;
using Talaryon.Toolbox.Extensions;

namespace stackmgr.Services;

public class ArgoCD
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

        if (env.ArgoCD.Namespace is null or "")
        {
            Console.WriteLine("No ArgoCD namespace provided. Please check your configuration.");
            return false;
        }

        if (env.ArgoCD.Service is null or "")
        {
            Console.WriteLine("No ArgoCD service provided. Please check your configuration.");
            return false;
        }

        return true;
    }

    public static async Task<bool> TestConnection(StackEnvironment env)
    {
        if (!CheckRequirements(env)) return false;
        
        var client = NewClient(env);
        var response = await client.GetAsync($"{env.RKE2.Url}/api/v1/namespaces/{env.ArgoCD.Namespace}/services/{env.ArgoCD.Service}/proxy/api/v1/applications");
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                Console.WriteLine("Unauthorized. Please check your access token.");
                break;
            case HttpStatusCode.NotFound:
                Console.WriteLine("Namespace or service not found. Please check your configuration.");
                break;
            default:
                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Success."
                    : $"Failed. Response code: {response.StatusCode}");
                break;
        }
        return response.IsSuccessStatusCode;
    }
}