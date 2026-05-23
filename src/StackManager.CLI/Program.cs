using System.CommandLine;
using System.Reflection;
using Talaryon.StackManager;
using Talaryon.Toolbox.Api;

var localConfig = LocalConfig.Get();

var rootCommand = new RootCommand();

// Auto-discover and register all commands that inherit from StackManagerCommand
var commandTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(StackManagerCommand)))
    .Select(type => (StackManagerCommand)Activator.CreateInstance(type)!)
    .ToList();

foreach (var command in commandTypes)
{
    rootCommand.Add(command);
}

try
{
    var parseResult = rootCommand.Parse("--help");
    if (args.Length > 0)
    {
        parseResult = rootCommand.Parse(args);
    }

    await parseResult.InvokeAsync(new InvocationConfiguration()
    {
        EnableDefaultExceptionHandler = false

    });
}
catch (ApiError error)
{
    LogMessage.AsError(error.Message ?? "Unknown error");   
}
catch (Exception ex)
{
    LogMessage.AsError(localConfig.DebugMode ? ex.ToString() : ex.Message);
}