using stackmgr.Arguments;
using stackmgr.Options;
using YamlDotNet.Serialization;

namespace stackmgr.Commands;

public class BuildCommand : StackManagerCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackArgument());
        SetAction(parseResult =>
        {
            var env = GetEnvironment<EnvironmentOption>(parseResult);
            var stack = GetStack<StackArgument>(parseResult, env);
            
            HelperMethods.LogInfo($"Building stack '{stack.Name}' in environment '{env.Name}'");
            
            BuildRegistryCredentials(stack);
            stack.SaveKustomization();
            HelperMethods.LogSuccess($"Stack '{stack.Name}' built.");
            HelperMethods.LogInfo("Run git commit and git push before stack sync.");
        });
    }

    private void BuildRegistryCredentials(Stack stack)
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "registry-credentials.yaml");
        var file = new FileInfo(path);
        
        if (stack.Environment.RegistryCredentials is { Length: > 0 })
        {
            var credentials = new RegistryCredentials();
            credentials.Metadata.Annotations.Path = stack.Environment.RegistryCredentials;
            HelperMethods.LogInfo($"Using registry credentials for stack '{stack.Name}'.");
            
            File.WriteAllText(file.FullName, new Serializer().Serialize(this));
        }
        else if (file.Exists)
        {
            file.Delete();
            HelperMethods.LogInfo($"Registry credentials for stack '{stack.Name}' are empty. {file.Name} removed.");
        }
    }
}