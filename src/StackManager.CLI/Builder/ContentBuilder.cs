namespace Talaryon.StackManager.Builder;

public interface IContentBuilder
{
    IContentBuilder With(string content);
    IContentBuilder With(FileInfo file);
    string Build();
}

public class ContentBuilder(StackApp app) : IContentBuilder
{
    private string _content = string.Empty;

    public IContentBuilder With(string content)
    {
        _content = content;
        return this;
    }

    public IContentBuilder With(FileInfo file)
    {
        _content = File.ReadAllText(file.FullName);
        return this;
    }

    public string Build()
    {
        var content = _content
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