using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Services;

public interface ITemplateService
{
    Task UpdateAsync(string branch = "main");
    IReadOnlyList<StackTemplate> GetTemplates();
    StackTemplate GetTemplate(string name);
    void ApplyTemplate(StackTemplate template, StackApp app);
}

public class TemplateService(IGitService git, LocalConfig config) : ITemplateService
{
    public async Task UpdateAsync(string branch)
    {
        var directory = new DirectoryInfo(StackTemplate.DirectoryName);
        var repo = git.Directory(directory);

        await (!repo.IsRepository ? repo.CloneAsync(config.AppRepository) : repo.PullAsync());
        await repo.CheckoutAsync(branch);
    }
    
    public IReadOnlyList<StackTemplate> GetTemplates()
    {
        var directory = new DirectoryInfo(StackTemplate.DirectoryName);
        var apps = directory
            .GetDirectories()
            .Select(v => new FileInfo(Path.Combine(v.FullName, StackTemplate.FileName)))
            .Where(v => v.Exists)
            .Select(StackResource.Load<StackTemplate>)
            .ToList();
            
        return apps;
    }

    public StackTemplate GetTemplate(string name)
    {
        var apps = GetTemplates();
        return apps.FirstOrDefault(v => v.Name == name) ?? throw new TemplateNotFoundException(name);
    }

    public void ApplyTemplate(StackTemplate template, StackApp app)
    {
        if (!(template.LocalDirectory.Exists && template.LocalFile.Exists))
        {
            throw new TemplateNotFoundException(template.Name);
        }

        if (!app.LocalDirectory.Exists)
            app.LocalDirectory.Create();

        var baseDirectory = new DirectoryInfo(Path.Combine(app.LocalDirectory.FullName, ".base"));
        if (baseDirectory.Exists)
        {
            baseDirectory.Delete(true);
        }
        baseDirectory.Create();

        var files = template.LocalDirectory
            .GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(v => v.Name != StackTemplate.FileName);

        foreach (var file in files.Where(v => v.Name.StartsWith("init.", StringComparison.OrdinalIgnoreCase)))
        {
            var destination = new FileInfo(Path.Combine(app.LocalDirectory.FullName, file.Name[5..]));
            if (destination.Exists) continue;

            var content = File.ReadAllText(file.FullName);
            
            content = ApplyVariables(content, app);
            
            File.WriteAllText(destination.FullName, content);
            LogMessage.AsInfo($"Applied init file '{destination.Name}' to app root.");
        }
        
        foreach(var file in files.Where(v => !v.Name.StartsWith("init.", StringComparison.OrdinalIgnoreCase)))
        {
            var destination = new FileInfo(Path.Combine(baseDirectory.FullName, file.Name));
            if (destination.Exists) continue;

            var content = File.ReadAllText(file.FullName);
            content = ApplyVariables(content, app);
            
            File.WriteAllText(destination.FullName, content);
            LogMessage.AsInfo($"Applied template file '{destination.Name}' to app base.");
        }
    }

    private string ApplyVariables(string content, StackApp app)
    {
        content = content
            .Replace("{{app-name}}", app.Name)
            .Replace("{{stack-name}}", app.Stack.Name)
            .Replace("{{env-name}}", app.Stack.Environment.Name)
            .Replace("{{vault-path}}", $"{app.Stack.Environment.Vault}/{app.Stack.Name}/{app.Name}");
                
        content = app.Volumes.Aggregate(content,
            (current, volume) => current.Replace("{{app-volume." + volume.Key + "}}", volume.Value));
        content = app.Params.Aggregate(content,
            (current, param) => current.Replace("{{app-param." + param.Key + "}}", param.Value));
        content = app.Requirements.Aggregate(content,
            (current, requirement) =>
                current.Replace("{{app-requirement." + requirement.Key + "}}", requirement.Value));
        
        return content;
    }
}