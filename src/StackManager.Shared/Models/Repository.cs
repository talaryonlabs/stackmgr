using System.Text.Json.Serialization;
using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("repositories", ApiEndpointType.Create)]
[ApiEndpoint("repositories/{name}", ApiEndpointType.Single | ApiEndpointType.Delete | ApiEndpointType.Update)]
public class Repository : IApiResource
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
    [JsonPropertyName("username")] public required string Username { get; set; }
    [JsonPropertyName("password")] public required string Password { get; set; }
}

[ApiEndpoint("repositories")]
public class RepositoryList : List<Repository>;