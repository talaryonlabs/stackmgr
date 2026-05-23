using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager;
using Talaryon.StackManager.DependencyInjection;
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

// Auto-discover and register all commands that inherit from StackManagerCommand
var commandTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(StackManagerCommand)))
    .Select(type => (StackManagerCommand)Activator.CreateInstance(type)!)
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
