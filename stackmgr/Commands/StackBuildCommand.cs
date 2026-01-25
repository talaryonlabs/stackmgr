using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackBuildCommand : Command
{
    public StackBuildCommand() : base("build", "Build a stack")
    {
        SetAction(v =>
        {
            var env = v.GetRequiredValue<StackEnvironment, EnvironmentOption>();
            var name = v.GetRequiredValue<string, NameArgument>();

            if (!env.HasStack(name))
            {
                Console.WriteLine($"Stack '{name}' does not exist in environment '{env}'");
                return;
            }
            
            var path = env.GetStackPath(name);
            var conf = StackConfig.Load(env, name);
            if (conf is null)
            {
                Console.WriteLine($"Stack '{name}' does not have a configuration file");
                return;
            }
            
            Console.WriteLine($"Building stack '{name}' in environment '{env}'");
            var kustomization = new Kustomization
            {
                Images = conf.Images?.Select(i => (KustomizationImage)i).ToList(),
                Resources = new DirectoryInfo(path)
                    .GetFiles("*.yaml", SearchOption.AllDirectories)
                    .Where(f => !new List<string> { Kustomization.FileName, StackConfig.FileName }.Contains(f.Name))
                    .Select(f => f.FullName.Replace(path, "").Replace("\\", "/")[1..])
                    .ToList()
            };
            
            kustomization.Save(env, name);
        });
    }
}