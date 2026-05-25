using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Extensions;
using Xunit;

namespace Talaryon.StackManager.Tests.Commands;

public class CommandDiscoveryTests
{
    private readonly Assembly _cliAssembly;

    public CommandDiscoveryTests()
    {
        _cliAssembly = typeof(BaseCommand).Assembly;
    }

    [Fact]
    public void CommandDiscovery_ShouldFindAllParentCommands()
    {
        // Get all non-generic, non-abstract commands that inherit from BaseCommand
        var commandTypes = _cliAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
            .Where(t =>
            {
                var baseType = t.BaseType;
                // Exclude generic resource commands
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
            "MigrateCommand",
            "RemoteCommand",
            "DefaultCommand"
        };

        foreach (var expected in expectedParentCommands)
        {
            Assert.Contains(commandTypes, t => t.Name == expected);
        }
    }

    [Fact]
    public void CommandDiscovery_ShouldFindResourceSubcommands()
    {
        // Get all resource subcommands (generic commands)
        var resourceCommands = _cliAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseCommand)))
            .Where(t =>
            {
                var baseType = t.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                    return false;
                var genericDef = baseType.GetGenericTypeDefinition();
                return genericDef == typeof(ResourceCreateCommand<,>)
                    || genericDef == typeof(ResourceDeleteCommand<,>)
                    || genericDef == typeof(ResourceDescribeCommand<,>)
                    || genericDef == typeof(ResourceGetCommand<>)
                    || genericDef == typeof(ResourceConfigureCommand<>);
            })
            .ToList();

        // These resource subcommands should exist
        var expectedResourceCommands = new[]
        {
            "NewVolumeCommand",
            "DeleteVolumeCommand", 
            "DescribeVolumeCommand",
            "GetVolumesCommand",
            "NewAppCommand",
            "DeleteAppCommand",
            "DescribeAppCommand",
            "GetAppsCommand",
            "ConfigureAppCommand",
            "NewStackCommand",
            "DeleteStackCommand",
            "DescribeStackCommand",
            "GetStacksCommand",
            "ConfigureStackCommand",
            "NewEnvironmentCommand",
            "DeleteEnvironmentCommand",
            "DescribeEnvironmentCommand",
            "GetEnvironmentsCommand",
            "ConfigureEnvironmentCommand",
            "NewIngressCommand",
            "DeleteIngressCommand",
            "DescribeIngressCommand",
            "GetIngressesCommand",
            "NewImageCommand",
            "DeleteImageCommand",
            "DescribeImageCommand",
            "GetImagesCommand"
        };

        foreach (var expected in expectedResourceCommands)
        {
            Assert.Contains(resourceCommands, t => t.Name == expected);
        }
    }

    [Fact]
    public void RootCommand_ShouldNotHaveDuplicateCommands()
    {
        // Create a simple service provider for testing
        var services = new ServiceCollection();
        services.AddSingleton<LocalConfig>(_ => LocalConfig.Get());
        services.AddHttpClient("ProxyService");
        services.AddStackManagerServices();
        var serviceProvider = services.BuildServiceProvider();

        // Build the root command with parent commands only
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
            command.SetServiceProvider(serviceProvider);
            rootCommand.Add(command);
        }

        // Parse should not throw duplicate key exception
        var exception = Record.Exception(() => rootCommand.Parse(Array.Empty<string>()));
        Assert.Null(exception);
    }
}
