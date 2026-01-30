using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackListCommand : Command
{
    public StackListCommand() : base("list", "List stacks")
    {
        SetAction(async v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentOption>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            Console.WriteLine($"Listing stacks for {env.Name}");

            var apps = await ArgoCD.ListApplicationsAsync(env);

            var test = apps.Select(app =>
            {
                return new []
                {
                    app.Metadata.Name,
                    app.Spec.Project,
                    app.Spec.Source.Path
                };
            }).ToList();
            
            test.Insert(0, new [] {"Name", "Project", "Path"});
            
            HelperMethods.PrintTable(test);

            return;
            
            var path = Path.Combine(Environment.CurrentDirectory, env.Name.ToLower());
            foreach (var stack in Directory.GetDirectories(path))
            {
                Console.WriteLine(Path.GetFileName(stack));
            }
        });
    }
}