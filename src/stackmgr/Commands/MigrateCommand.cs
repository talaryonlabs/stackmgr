using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class MigrateCommand : StackManagerCommand
{
    public MigrateCommand() : base("migrate", "Migrate an resource (app)")
    {
        var app = new StackManagerCommand("app", "Migrate an app from a template")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new AppArgument(),
            new WithoutIngressOption()
        };
        app.SetAction(MigrateApp);
        
        Add(app);
    }

    private async Task MigrateApp(ParseResult v)
    {
        var env = GetEnvironment<EnvironmentOption>(v);
        var stack = GetStack<StackArgument>(v, env);
        var app = GetApp<AppArgument>(v, stack);
        
        
        HelperMethods.LogInfo($"Migrating app '{app.Name}' from template '{app.Template}' ({stack.Name} in environment '{env.Name}')");

        var appService = new AppService(stack, app);
        await appService.Migrate(new AppServiceOptions()
        {
            WithoutIngress = v.GetValue<bool, WithoutIngressOption>()
        });
        
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess("Migration done.");
    }
    
}