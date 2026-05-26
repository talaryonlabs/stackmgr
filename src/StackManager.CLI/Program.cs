using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Services;

// Check for --version or -v before DI setup
if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))
{
    var assembly = typeof(Program).Assembly;
    var version = assembly.GetName().Version?.ToString() ?? "unknown";
    var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? version;
    Console.WriteLine($"stackmgr version {informationalVersion}");
    Environment.Exit(0);
}

// Setup DI container
var services = new ServiceCollection()
    .AddStackManagerServices()
    .BuildServiceProvider();


// Get local config
var localConfig = services.GetRequiredService<LocalConfig>();

var rootCommand = new RootCommand();

// Auto-discover and register all commands that inherit from BaseCommand
var commands = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
    .ToList();

// Exclude resource subcommands (ResourceCreateCommand, ResourceDeleteCommand, etc.) 
// as they are added under their parent commands (NewCommand, DeleteCommand, etc.)
commands.Where(t =>
    {
        var baseType = t.BaseType;
        if (baseType is not { IsGenericType: true })
            return true;
        var genericDef = baseType.GetGenericTypeDefinition();
        return genericDef != typeof(ResourceCreateCommand<,>)
               && genericDef != typeof(ResourceDeleteCommand<,>)
               && genericDef != typeof(ResourceDescribeCommand<,>)
               && genericDef != typeof(ResourceGetCommand<>)
               && genericDef != typeof(ResourceConfigureCommand<>)
               && genericDef != typeof(ResourceMigrateCommand<,>);
    })
    .Select(type => (BaseCommand)Activator.CreateInstance(type)!)
    .ToList()
    .ForEach(command => 
    {
        command.SetServiceProvider(services);
        rootCommand.Add(command);
    });

await services.GetRequiredService<IGitService>()
    .CurrentDirectory()
    .AddIgnoreEntriesAsync([
        ".stackmgr", 
        ".apps",
        ".env", 
        ".validation"
    ]);

var errorService = services.GetRequiredService<IErrorService>();

try
{
    var parseResult = rootCommand.Parse(args);
    parseResult.Invoke();
}
catch (Exception e)
{
    errorService.SetExitCode(1);
    errorService.LogError(e);   
}

if (errorService.ExitCode > 0)
{
    if (errorService.LastException is null)
    {
        throw new Exception("Unknown error occurred.");   
    }

    LogMessage.AsError(localConfig.DebugMode
        ? errorService.LastException.ToString()
        : errorService.LastException.Message);

    Environment.Exit(errorService.ExitCode);
}
