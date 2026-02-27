using System.IO.Pipes;
using System.Text.RegularExpressions;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackAppOptions
{
    public string? Volume { get; set; }
    public string? Host { get; set; }
    public short Port { get; set; }
    public string[] Config { get; set; } = [];
    public string? Template { get; set; }
    
}

public class StackApp : IStackObject
{
    public static StackApp Create(Stack stack, string name, StackAppOptions options)
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
            Volume = options.Volume ?? "",
            Host = options.Host ?? "",
            Port = options.Port,
            Template = options.Template ?? "",
            Config = options.Config.Select(x =>
            {
                var config = x.Split("=");
                return new StackAppConfig
                {
                    Name = config[0]
                        .Trim(),
                    Value = config.Length > 1
                        ? config[1]
                            .Trim()
                        : ""
                };
            })
            .ToList()
            
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

    private async Task CheckRequirements(FileSystemInfo[] files)
    {
        var errors = new Dictionary<string, string>();
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.FullName);
            if (content.Contains("{{vault-path}}") && string.IsNullOrEmpty(Stack.Environment.Vault))
            {
                errors.TryAdd("vault-path", "Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
            }
            if (content.Contains("{{app-volume}}") && string.IsNullOrEmpty(Volume))
            {
                errors.TryAdd("app-volume", "Parameter --volume is required for this template.");
            }
            if (content.Contains("{{app-host}}") && string.IsNullOrEmpty(Host))
            {
                errors.TryAdd("app-host", "Parameter --host is required for this template.");
            }
            if (content.Contains("{{app-port}}") && Port == 0)
            {
                errors.TryAdd("app-port", "Parameter --port is required for this template.");
            }
            
            var regex = new Regex(@"\{\{config\.([A-z]+)\}\}", RegexOptions.IgnoreCase);
            var conf = regex.Matches(content);
            
            foreach (Match match in conf)
            {
                if (match is { Success: true } && !Config.Any(x => x.Name.Equals(match.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase)))
                {
                    errors.TryAdd(match.Groups[1].Value, $"Parameter --config \"{match.Groups[1].Value}=<value>\" is required for this template.");
                }
            }
        }

        if(errors.Count == 0) return;
        foreach (var error in errors)
        {
            LogMessage.AsError($"- {error.Value}");
        }
        throw new Exception("Aborted.");
    }
    
    public async Task Migrate()
    {
        var git = new GitService(Stack.Environment);
        var apps = await git.GetAppsAsync(Branch);
        var template = apps.FirstOrDefault(x => x.Name.Equals(TemplateName, StringComparison.CurrentCultureIgnoreCase));
        if (template is null)
        {
            throw new TemplateNotFoundException(TemplateName);
        }

        var files = template.GetFileSystemInfos("*", SearchOption.AllDirectories);
        var vault = Stack.Environment.Vault.EndsWith("/")
            ? Stack.Environment.Vault[..^1]
            : Stack.Environment.Vault;

        await CheckRequirements(files);

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

            if (!HelperMethods.ConfirmWarning("Do you want to migrate all files?"))
            {
                LogMessage.AsInfo("Aborted.");
                return;
            }
        }
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.FullName);
            
            content = content
                .Replace("{{app-name}}", Name)
                .Replace("{{stack-name}}", Stack.Name);
            
            if (content.Contains("{{vault-path}}")) content = content.Replace("{{vault-path}}", $"{vault}/{Stack.Name}/{Name}");
            if (content.Contains("{{app-volume}}")) content = content.Replace("{{app-volume}}", Volume);
            if (content.Contains("{{app-host}}")) content = content.Replace("{{app-host}}", Host);
            if (content.Contains("{{app-port}}")) content = content.Replace("{{app-port}}", Port.ToString());

            content = Config.Aggregate(content, (current, config) => current.Replace("{{config." + config.Name + "}}", config.Value));

            await File.WriteAllTextAsync(Path.Combine(LocalDirectory.FullName, file.Name), content);
            LogMessage.AsInfo($"Applied '{file.Name}'.");
        }
    }
    
    [YamlIgnore] public required Stack Stack { get; set; }
    [YamlIgnore]
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, Name)
    );
    [YamlIgnore] public string Branch => Template.Contains(':') ? Template.Split(":")[0] : "prod";
    [YamlIgnore] public string TemplateName => Template.Contains(':') ? Template.Split(":")[1] : Template;
    
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "volume")] public string Volume { get; init; } = "";
    [YamlMember(Alias = "template")] public string Template { get; init; } = "";
    [YamlMember(Alias = "host")] public string Host { get; init; } = "";
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "config")] public List<StackAppConfig> Config { get; init; } = [];
}

public class StackAppConfig
{
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "value")] public required string Value { get; set; }
}