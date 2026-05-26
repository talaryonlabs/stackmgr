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
            .Select(v => v.GetFile(StackTemplate.FileName))
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

        var baseDirectory = app.LocalDirectory.GetDirectory(".base");
        if (!baseDirectory.Exists)
            baseDirectory.Create();
        else
            baseDirectory
                .GetFiles("template.*.yaml", SearchOption.TopDirectoryOnly)
                .ToList()
                .ForEach(v => v.Delete());


        var files = template.LocalDirectory
            .GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(v => v.Name != StackTemplate.FileName)
            .ToList();

        foreach (var file in files)
        {
            var destination = GetDestinationFileInfo(baseDirectory, file.Name);
            if (destination.Exists) continue;
            
            var content = File.ReadAllText(file.FullName);
            File.WriteAllText(destination.FullName, content);
            LogMessage.AsInfo($"Applied file '{destination.Name}' to .base sub-directory.");
        }
    }

    private static FileInfo GetDestinationFileInfo(DirectoryInfo destination, string filename) =>
        new(Path.Combine(destination.FullName,
            filename.StartsWith("init.", StringComparison.OrdinalIgnoreCase) ? filename : $"template.{filename}"));
}