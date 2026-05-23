using Talaryon.StackManager.Exceptions;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackApp : IStackObject
{
    public static StackApp Create(Stack stack, string name, StackAppTemplate? template)
    {
        var existing = stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            throw new AppAlreadyExistsException(stack, existing);
        }

        var app = new StackApp
        {
            Stack = stack,
            Name = name,
            Template = template
        };

        lock (stack.Apps)
        {
            stack.Apps.Add(app);
        }

        stack.SaveConfig();

        return app;
    }

    public void Delete()
    {
        if (LocalDirectory.Exists)
        {
            LocalDirectory.Delete(true);
        }

        lock (Stack.Apps)
        {
            Stack.Apps.Remove(this);
        }

        Stack.SaveConfig();
    }

    public async Task<bool> CheckRequirements(StackTemplate template)
    {
        var errors = new Dictionary<string, string>();
        var files = template.LocalDirectory
            .GetFileSystemInfos("*", SearchOption.AllDirectories);

        foreach (var requirement in Requirements.Where(requirement =>
                     !Stack.Apps.Exists(v => v.Name == requirement.Value)))
        {
            errors.TryAdd(requirement.Key, $"Required app '{requirement.Value}' not found in stack.");
        }

        foreach (var volume in Volumes.Where(volume => !Stack.Volumes.Exists(v => v.Name == volume.Value)))
        {
            errors.TryAdd(volume.Key, $"Required volume '{volume.Value}' not found in stack.");
        }

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.FullName);
            if (content.Contains("{{vault-path}}") && string.IsNullOrEmpty(Stack.Environment.Vault))
            {
                errors.TryAdd("vault-path",
                    "Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
            }
        }

        if (errors.Count == 0) return true;
        foreach (var error in errors)
        {
            LogMessage.AsError($"- {error.Value}");
        }

        return false;
    }

    [YamlIgnore] public required Stack Stack { get; set; }

    [YamlIgnore]
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, Name)
    );

    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "images")] public Dictionary<string, string> Images { get; init; } = [];
    [YamlMember(Alias = "volumes")] public Dictionary<string, string>  Volumes { get; init; } = [];
    [YamlMember(Alias = "requirements")] public Dictionary<string, string> Requirements { get; init; } = [];
    [YamlMember(Alias = "params")] public Dictionary<string, string> Params { get; init; } = [];
    [YamlMember(Alias = "template")] public StackAppTemplate? Template { get; init; }
}

public class StackAppTemplate
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "branch")] public required string Branch { get; init; }
}