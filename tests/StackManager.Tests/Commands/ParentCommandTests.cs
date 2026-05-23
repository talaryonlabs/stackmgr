using System.CommandLine;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Commands.Volumes;
using Xunit;

namespace Talaryon.StackManager.Tests.Commands;

public class ParentCommandTests
{
    [Fact]
    public void NewCommand_ShouldHaveVolumeSubcommand()
    {
        var newCommand = new NewCommand();
        
        // Find the volume subcommand
        var volumeCommand = newCommand.Subcommands.FirstOrDefault(c => c.Name == "volume");
        Assert.NotNull(volumeCommand);
        Assert.IsType<NewVolumeCommand>(volumeCommand);
    }

    [Fact]
    public void DeleteCommand_ShouldHaveVolumeSubcommand()
    {
        var deleteCommand = new DeleteCommand();
        
        var volumeCommand = deleteCommand.Subcommands.FirstOrDefault(c => c.Name == "volume");
        Assert.NotNull(volumeCommand);
        Assert.IsType<DeleteVolumeCommand>(volumeCommand);
    }

    [Fact]
    public void DescribeCommand_ShouldHaveVolumeSubcommand()
    {
        var describeCommand = new DescribeCommand();
        
        var volumeCommand = describeCommand.Subcommands.FirstOrDefault(c => c.Name == "volume");
        Assert.NotNull(volumeCommand);
        Assert.IsType<DescribeVolumeCommand>(volumeCommand);
    }

    [Fact]
    public void GetCommand_ShouldHaveVolumesSubcommand()
    {
        var getCommand = new GetCommand();
        
        var volumesCommand = getCommand.Subcommands.FirstOrDefault(c => c.Name == "volumes");
        Assert.NotNull(volumesCommand);
        Assert.IsType<GetVolumesCommand>(volumesCommand);
    }

    [Fact]
    public void NewCommand_ShouldNotHaveDuplicateSubcommands()
    {
        var newCommand = new NewCommand();
        var subcommandNames = newCommand.Subcommands.Select(c => c.Name).ToList();
        
        Assert.Equal(subcommandNames.Count, subcommandNames.Distinct().Count());
    }

    [Fact]
    public void DeleteCommand_ShouldNotHaveDuplicateSubcommands()
    {
        var deleteCommand = new DeleteCommand();
        var subcommandNames = deleteCommand.Subcommands.Select(c => c.Name).ToList();
        
        Assert.Equal(subcommandNames.Count, subcommandNames.Distinct().Count());
    }

    [Fact]
    public void DescribeCommand_ShouldNotHaveDuplicateSubcommands()
    {
        var describeCommand = new DescribeCommand();
        var subcommandNames = describeCommand.Subcommands.Select(c => c.Name).ToList();
        
        Assert.Equal(subcommandNames.Count, subcommandNames.Distinct().Count());
    }

    [Fact]
    public void ConfigureCommand_ShouldNotHaveDuplicateSubcommands()
    {
        var configureCommand = new ConfigureCommand();
        var subcommandNames = configureCommand.Subcommands.Select(c => c.Name).ToList();
        
        Assert.Equal(subcommandNames.Count, subcommandNames.Distinct().Count());
    }

    [Fact]
    public void ParentCommands_ShouldHaveAllExpectedResourceSubcommands()
    {
        var newCommand = new NewCommand();
        var deleteCommand = new DeleteCommand();
        var describeCommand = new DescribeCommand();
        var getCommand = new GetCommand();
        var configureCommand = new ConfigureCommand();

        // All parent commands should have consistent subcommands
        var resourceTypes = new[] { "volume", "app", "stack", "environment", "ingress", "image" };

        foreach (var resource in resourceTypes)
        {
            // new, delete, describe should have singular form
            Assert.Contains(newCommand.Subcommands, c => c.Name == resource);
            Assert.Contains(deleteCommand.Subcommands, c => c.Name == resource);
            Assert.Contains(describeCommand.Subcommands, c => c.Name == resource);
        }

        // get should have plural form for most resources
        var pluralResources = new[] { "volumes", "apps", "stacks", "environments", "ingresses", "images" };
        foreach (var resource in pluralResources)
        {
            Assert.Contains(getCommand.Subcommands, c => c.Name == resource);
        }

        // configure should have singular form
        foreach (var resource in new[] { "stack", "app", "environment" })
        {
            Assert.Contains(configureCommand.Subcommands, c => c.Name == resource);
        }
    }

    [Fact]
    public void VolumeCommands_ShouldHaveCorrectNames()
    {
        // NewVolumeCommand should use "volume" as name
        var newVolumeCommand = new NewVolumeCommand();
        Assert.Equal("volume", newVolumeCommand.Name);

        // DeleteVolumeCommand should use "volume" as name
        var deleteVolumeCommand = new DeleteVolumeCommand();
        Assert.Equal("volume", deleteVolumeCommand.Name);

        // DescribeVolumeCommand should use "volume" as name
        var describeVolumeCommand = new DescribeVolumeCommand();
        Assert.Equal("volume", describeVolumeCommand.Name);

        // GetVolumesCommand should use "volumes" as name (plural)
        var getVolumesCommand = new GetVolumesCommand();
        Assert.Equal("volumes", getVolumesCommand.Name);
    }
}
