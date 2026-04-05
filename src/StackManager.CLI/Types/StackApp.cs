using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackApp : IStackObject
{
    public static StackApp Create(Stack stack, string name, StackAppTemplate? template)
    {
        var existing = stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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

        foreach (var requirement in Requirements.Where(requirement => !Stack.Apps.Exists(v => v.Name == requirement.Value)))
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
                errors.TryAdd("vault-path", "Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
            }
        }
        
        if(errors.Count == 0) return true;
        foreach (var error in errors)
        {
            LogMessage.AsError($"- {error.Value}");
        }

        return false;
    }
    
    public async Task Migrate(StackTemplate template)
    {
        var files = template.LocalDirectory
            .GetFileSystemInfos("*", SearchOption.AllDirectories);
        
        var vault = Stack.Environment.Vault.EndsWith("/")
            ? Stack.Environment.Vault[..^1]
            : Stack.Environment.Vault;

        if (!LocalDirectory.Exists)
        {
            LocalDirectory.Create();
        }
        else
        {
            LogMessage.AsInfo("The following files will be migrated:");
            foreach (var existing in files.Where(v => File.Exists(Path.Combine(LocalDirectory.FullName, v.Name))).ToList())
            {
                LogMessage.AsWarning($"- {existing.Name} (replace)");
            }
        
            foreach (var add in files.Where(v => !File.Exists(Path.Combine(LocalDirectory.FullName, v.Name))).ToList())
            {
                LogMessage.AsSuccess($"- {add.Name} (add)");
            }
            
            if (!LogMessage.AsConfirmWarning("Do you want to migrate all files?"))
            {
                LogMessage.AsInfo("Aborted.");
                return;
            }
        }
        
        foreach (var file in files)
        {
            if(file.Name.Equals(StackTemplate.FileName, StringComparison.InvariantCultureIgnoreCase)) continue;
            
            var content = await File.ReadAllTextAsync(file.FullName);
            
            content = content
                .Replace("{{app-name}}", Name)
                .Replace("{{stack-name}}", Stack.Name)
                .Replace("{{env-name}}", Stack.Environment.Name)
                .Replace("{{vault-path}}", $"{vault}/{Stack.Name}/{Name}");

            content = Volumes.Aggregate(content, (current, volume) => current.Replace("{{app-volume." + volume.Key + "}}", volume.Value));
            content = Params.Aggregate(content, (current, param) => current.Replace("{{app-param." + param.Key + "}}", param.Value));
            content = Requirements.Aggregate(content, (current, requirement) => current.Replace("{{app-requirement." + requirement.Key + "}}", requirement.Value));

            await File.WriteAllTextAsync(Path.Combine(LocalDirectory.FullName, file.Name), content);
            LogMessage.AsInfo($"Applied '{file.Name}'.");
        }
    }
    
    [YamlIgnore] public required Stack Stack { get; set; }
    [YamlIgnore]
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, Name)
    );
    
    [YamlMember(Alias = "name")] public required string Name { get; init; }
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