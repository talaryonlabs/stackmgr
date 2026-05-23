using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Commands.Volumes;
using Xunit;

namespace Talaryon.StackManager.Tests.Commands;

public class CommandDiscoveryTests
{
    private readonly Assembly _cliAssembly;
    private readonly IServiceProvider _serviceProvider;

    public CommandDiscoveryTests()
    {
        _cliAssembly = typeof(BaseCommand).Assembly;
        var services = new ServiceCollection();
        services.AddSingleton<LocalConfig>(_ => LocalConfig.Get());
        services.AddHttpClient();
        services.AddStackManagerServices();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void CommandDiscovery_ShouldExcludeResourceSubcommands()
    {
        // Simulate the command discovery logic from Program.cs
        var commandTypes = _cliAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
            .Where(t =>
            {
                var baseType = t.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                    return true;
                var genericDef = baseType.GetGenericTypeDefinition();
                return genericDef != typeof(ResourceCreateCommand<,>)
                    && genericDef != typeof(ResourceDeleteCommand<,>)
                    && genericDef != typeof(ResourceDescribeCommand<,>)
                    && genericDef != typeof(ResourceGetCommand<>)
                    && genericDef != typeof(ResourceConfigureCommand<>);
            })
            .ToList();

        // These parent commands should be included
        var expectedParentCommands = new[]
        {
            "NewCommand",
            "GetCommand", 
            "DeleteCommand",
            "DescribeCommand",
            "ConfigureCommand",
            "BuildCommand",
            "SyncCommand",
            "MoveCommand",
            "MigrateCommand",
            "RemoteCommand",
            "DefaultCommand"
        };

        foreach (var expected in expectedParentCommands)
        {
            Assert.Contains(commandTypes, t => t.Name == expected);
        }

        // These resource subcommands should be EXCLUDED
        var excludedSubcommands = new[]
        {
            "NewVolumeCommand",
            "DeleteVolumeCommand", 
            "DescribeVolumeCommand",
            "GetVolumesCommand",
            "NewAppCommand",
            "DeleteAppCommand",
            "DescribeAppCommand",
            "GetAppsCommand",
            "NewStackCommand",
            "DeleteStackCommand",
            "DescribeStackCommand",
            "GetStacksCommand",
            "NewEnvironmentCommand",
            "DeleteEnvironmentCommand",
            "DescribeEnvironmentCommand",
            "GetEnvironmentsCommand",
            "NewIngressCommand",
            "DeleteIngressCommand",
            "DescribeIngressCommand",
            "GetIngressesCommand",
            "NewImageCommand",
            "DeleteImageCommand",
            "DescribeImageCommand",
            "GetImagesCommand",
            "ConfigureStackCommand",
            "ConfigureAppCommand",
            "ConfigureEnvironmentCommand"
        };

        foreach (var excluded in excludedSubcommands)
        {
            Assert.DoesNotContain(commandTypes, t => t.Name == excluded);
        }
    }

    [Fact]
    public void RootCommand_ShouldNotHaveDuplicateVolumeCommands()
    {
        // Build the root command as Program.cs does
        var rootCommand = new RootCommand();
        
        var commandTypes = _cliAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
            .Where(t =>
            {
                var baseType = t.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                    return true;
                var genericDef = baseType.GetGenericTypeDefinition();
                return genericDef != typeof(ResourceCreateCommand<,>)
                    && genericDef != typeof(ResourceDeleteCommand<,>)
                    && genericDef != typeof(ResourceDescribeCommand<,>)
                    && genericDef != typeof(ResourceGetCommand<>)
                    && genericDef != typeof(ResourceConfigureCommand<>);
            })
            .Select(type => (BaseCommand)Activator.CreateInstance(type)!)
            .ToList();

        foreach (var command in commandTypes)
        {
            command.SetServiceProvider(_serviceProvider);
            rootCommand.Add(command);
        }

        // Parse should not throw duplicate key exception
        var exception = Record.Exception(() => rootCommand.Parse(Array.Empty<string>()));
        Assert.Null(exception);
    }

    [Fact]
    public void RootCommand_ParsingShouldNotThrowForAllResourceTypes()
    {
        var rootCommand = new RootCommand();
        
        var commandTypes = _cliAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
            .Where(t =>
            {
                var baseType = t.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                    return true;
                var genericDef = baseType.GetGenericTypeDefinition();
                return genericDef != typeof(ResourceCreateCommand<,>)
                    && genericDef != typeof(ResourceDeleteCommand<,>)
                    && genericDef != typeof(ResourceDescribeCommand<,>)
                    && genericDef != typeof(ResourceGetCommand<>)
                    && genericDef != typeof(ResourceConfigureCommand<>);
            })
            .Select(type => (BaseCommand)Activator.CreateInstance(type)!)
            .ToList();

        foreach (var command in commandTypes)
        {
            command.SetServiceProvider(_serviceProvider);
            rootCommand.Add(command);
        }

        // Test parsing for various resource type commands
        var testInputs = new[]
        {
            new[] { "new", "volume", "--help" },
            new[] { "delete", "volume", "--help" },
            new[] { "describe", "volume", "--help" },
            new[] { "get", "volumes", "--help" },
            new[] { "new", "app", "--help" },
            new[] { "delete", "app", "--help" },
            new[] { "describe", "app", "--help" },
            new[] { "get", "apps", "--help" },
            new[] { "new", "stack", "--help" },
            new[] { "delete", "stack", "--help" },
            new[] { "describe", "stack", "--help" },
            new[] { "get", "stacks", "--help" },
        };

        foreach (var input in testInputs)
        {
            var exception = Record.Exception(() => rootCommand.Parse(input));
            Assert.Null(exception);
        }
    }
}
