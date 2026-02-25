using Talaryon.Toolbox.Api;

namespace StackManager.Shared.Models;

[ApiEndpoint("namespaces")]
public class Namespace
{
    public required string Name { get; set; }
    public string? Project { get; set; }
}