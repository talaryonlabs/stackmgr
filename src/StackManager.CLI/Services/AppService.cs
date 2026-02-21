using System.Text.RegularExpressions;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Services;

public class AppServiceOptions
{
    public bool WithoutIngress { get; set; }
}

public class AppService(Stack stack, StackApp app)
{
    private string AppPath => Path.Combine(stack.LocalDirectory.FullName, app.Name);

    public async Task Install(AppServiceOptions serviceOptions)
    {
        var files = await GetFiles();
        await CheckRequirements(files, serviceOptions);
        await MigrateFiles(files, serviceOptions);
    }

    public async Task Migrate(AppServiceOptions serviceOptions)
    {
        var files = await GetFiles();

        HelperMethods.LogInfo("The following files will be migrated:");
        foreach (var existing in files.Where(v => File.Exists(Path.Combine(AppPath, v.Name))).ToList())
        {
            HelperMethods.LogWarning($"- {existing.Name} (replace)");
        }
        
        foreach (var add in files.Where(v => !File.Exists(Path.Combine(AppPath, v.Name))).ToList())
        {
            HelperMethods.LogSuccess($"- {add.Name} (add)");
        }

        if (!HelperMethods.ConfirmWarning("Do you want to migrate all files?"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }
        
        await CheckRequirements(files, serviceOptions);
        await MigrateFiles(files, serviceOptions);
    }

    private async Task<FileSystemInfo[]> GetFiles()
    {
        var infos = app.Template.Split(":");
        var git = new GitService(stack.Environment);
        var apps = await git.GetAppsAsync(infos[0]);
        
        HelperMethods.LogInfo("");
        
        var template = apps.FirstOrDefault(x => x.Name.Equals(infos[1], StringComparison.CurrentCultureIgnoreCase));
        return template is null ? throw new TemplateNotFoundException(infos[1]) : template.GetFileSystemInfos("*", SearchOption.AllDirectories);
    }

    private async Task CheckRequirements(FileSystemInfo[] files, AppServiceOptions serviceOptions)
    {
        foreach (var file in files)
        {
            if (file.Name.StartsWith("ingress.") && serviceOptions.WithoutIngress) continue;
            
            var content = await File.ReadAllTextAsync(file.FullName);
            if (content.Contains("{{vault-path}}") && string.IsNullOrEmpty(stack.Environment.Vault))
            {
                throw new Exception("Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
            }
            if (content.Contains("{{app-volume}}") && string.IsNullOrEmpty(app.Volume))
            {
                throw new Exception("Parameter --volume is required for this template.");
            }
            if (content.Contains("{{app-host}}") && string.IsNullOrEmpty(app.Host))
            {
                throw new Exception("Parameter --host is required for this template.");
            }
            if (content.Contains("{{app-port}}") && app.Port == 0)
            {
                throw new Exception("Parameter --port is required for this template.");
            }

            var regex = new Regex(@"\{\{config\.([A-z]+)\}\}", RegexOptions.IgnoreCase);
            var conf = regex.Matches(content);
            foreach (Match match in conf)
            {
                if (match is { Success: true } && !app.Config.Any(x => x.Name.Equals(match.Groups[1].Value, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception($"Parameter --config \"{match.Groups[1].Value}=<value>\" is required for this template.");
                }
            }
        }
    }
    
    private async Task MigrateFiles(FileSystemInfo[] files, AppServiceOptions serviceOptions)
    {
        foreach (var file in files)
        {
            if (file.Name.StartsWith("ingress.") && serviceOptions.WithoutIngress) continue;
            
            var content = await File.ReadAllTextAsync(file.FullName);
            var vault = stack.Environment.Vault.EndsWith("/")
                ? stack.Environment.Vault[..^1]
                : stack.Environment.Vault;

            content = content
                .Replace("{{app-name}}", app.Name)
                .Replace("{{stack-name}}", stack.Name);
            
            if (content.Contains("{{vault-path}}")) content = content.Replace("{{vault-path}}", $"{vault}/{stack.Name}/{app.Name}");
            if (content.Contains("{{app-volume}}")) content = content.Replace("{{app-volume}}", app.Volume);
            if (content.Contains("{{app-host}}")) content = content.Replace("{{app-host}}", app.Host);
            if (content.Contains("{{app-port}}")) content = content.Replace("{{app-port}}", app.Port.ToString());

            content = app.Config.Aggregate(content, (current, config) => current.Replace("{{config." + config.Name + "}}", config.Value));

            await File.WriteAllTextAsync(Path.Combine(AppPath, file.Name), content);
            HelperMethods.LogInfo($"Applied '{file.Name}'.");
        }
    }
}