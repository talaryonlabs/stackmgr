using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackRedirect : IStackObject
{
    public const string DirectoryName = ".redirects";

    public static StackRedirect Create(Stack stack, string hostname)
    {
        var redirect = new StackRedirect
        {
            Stack = stack,
            Name = HelperMethods.HostToName(hostname),
            Hostname = hostname
        };

        lock (stack.Redirects)
        {
            stack.Redirects.Add(redirect);
        }
        stack.SaveConfig();

        return redirect;
    }
    
    public async Task Migrate(FileSystemInfo[] files)
    {
        if (!LocalDirectory.Exists)
        {
            LocalDirectory.Create();
        }
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.FullName);
            
            content = content
                .Replace("{{app-name}}", Name)
                .Replace("{{app-host}}", Hostname)
                .Replace("{{stack-name}}", Stack.Name);
            
            await File.WriteAllTextAsync(Path.Combine(LocalDirectory.FullName, file.Name), content);
            LogMessage.AsInfo($"Applied '{file.Name}'.");
        }
    }
    
    public void Delete()
    {
        if (LocalDirectory.Exists)
        {
            LocalDirectory.Delete(true);
        }
        lock (Stack.Redirects)
        {
            Stack.Redirects.Remove(this);
        }
        Stack.SaveConfig();
    }
    
    [YamlIgnore] public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, DirectoryName, Name)
    );
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "hostname")] public required string Hostname { get; init; }
}