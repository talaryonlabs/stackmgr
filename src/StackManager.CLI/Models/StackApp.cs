using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackApp : IStackObject
{
    [YamlIgnore] public required Stack Stack { get; set; }
    [YamlIgnore]
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, Name)
    );

    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "images")] public Dictionary<string, string> Images { get; init; } = [];
    [YamlMember(Alias = "volumes")] public Dictionary<string, string>  Volumes { get; init; } = [];
    [YamlMember(Alias = "requirements")] public Dictionary<string, string> Requirements { get; init; } = [];
    [YamlMember(Alias = "params")] public Dictionary<string, string> Params { get; init; } = [];
    [YamlMember(Alias = "template")] public StackAppTemplate? Template { get; set; }
}

/// <summary>
/// Represents a template reference for a StackApp.
/// Supports both new object format (name, branch) and old string format for backward compatibility.
/// String format: "name@branch", "name:branch", or just "name" (defaults to "main")
/// </summary>
public class StackAppTemplate
{
    [YamlMember(Alias = "name")] public string Name { get; set; } = "";
    [YamlMember(Alias = "branch")] public string Branch { get; set; } = "main";

    /// <summary>
    /// Default constructor for YAML deserialization.
    /// </summary>
    public StackAppTemplate() { }

    /// <summary>
    /// Constructor that accepts a string template reference.
    /// String format: "name@branch", "name:branch", or just "name" (defaults to "main")
    /// </summary>
    public StackAppTemplate(string templateString)
    {
        if (string.IsNullOrWhiteSpace(templateString))
            throw new ArgumentException("Template string cannot be null or empty", nameof(templateString));
        
        var parts = templateString.Split(['@', ':'], 2);
        Name = parts[0].Trim();
        Branch = parts.Length > 1 ? parts[1].Trim() : "main";
    }

    /// <summary>
    /// Creates a StackAppTemplate from a string.
    /// String format: "name@branch", "name:branch", or just "name" (defaults to "main")
    /// </summary>
    public static StackAppTemplate FromString(string templateString)
    {
        return new StackAppTemplate(templateString);
    }

    /// <summary>
    /// Implicit conversion from string to StackAppTemplate for backward compatibility.
    /// </summary>
    public static implicit operator StackAppTemplate(string templateString) => new(templateString);
}
