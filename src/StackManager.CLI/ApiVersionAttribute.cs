namespace Talaryon.StackManager;

public interface IApiVersionItem
{
    string? Version { get; set; }
}

public class ApiVersionAttribute(string? version = null) : Attribute
{
    public bool IsDefault => version == null;
    public string? Version => version;   
}