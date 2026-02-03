using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackListCommand : StackManagerCommand
{
    public StackListCommand() : base("list", "List stacks")
    {
        SetAction(async v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            
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