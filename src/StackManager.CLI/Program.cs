using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;
using Talaryon.Toolbox.Api;

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
var services = new ServiceCollection();
services.AddSingleton<LocalConfig>(_ => LocalConfig.Get());
services.AddHttpClient();
services.AddStackManagerServices();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get local config
var localConfig = serviceProvider.GetRequiredService<LocalConfig>();

var rootCommand = new RootCommand();

// Auto-discover and register all commands that inherit from BaseCommand
// Exclude resource subcommands (ResourceCreateCommand, ResourceDeleteCommand, etc.) 
// as they are added under their parent commands (NewCommand, DeleteCommand, etc.)
var commandTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
    .Where(t => 
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
    .ToList();

foreach (var command in commandTypes)
{
    // Inject service provider into each command
    command.SetServiceProvider(serviceProvider);
    rootCommand.Add(command);
}

int exitCode = 0;

try
{
    var parseResult = rootCommand.Parse(args);
    parseResult.Invoke();
}
catch (CliException cliEx)
{
    LogMessage.AsError(cliEx.Message);
    if (localConfig.DebugMode && cliEx.ShowStackTrace)
    {
        Console.Error.WriteLine(cliEx.StackTrace);
    }
    exitCode = cliEx.ExitCode;
}
catch (ApiError apiError)
{
    LogMessage.AsError(apiError.Message ?? "Unknown API error");
    exitCode = 2;
}
catch (Exception ex)
{
    LogMessage.AsError(localConfig.DebugMode ? ex.ToString() : ex.Message);
    exitCode = 2;
}

Environment.Exit(exitCode);
