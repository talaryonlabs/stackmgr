using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Models;
using Xunit;

namespace Talaryon.StackManager.Tests.Models;

public class StackEnvironmentTests
{
    [Fact]
    public void StackEnvironment_ShouldHaveDefaultValues()
    {
        var env = new StackEnvironment();

        Assert.Null(env.Name);
        Assert.Equal("environment.talaryon.io/v1beta", env.Version);
        Assert.Null(env.Vault);
        Assert.Null(env.Outpost);
        Assert.Null(env.CertIssuer);
        Assert.Null(env.RegistryCredentials);
        Assert.Null(env.Repository);
        Assert.Null(env.Remote);
        Assert.False(env.IsDeleted);
    }

    [Fact]
    public void StackEnvironment_WithNameSet_ShouldHaveCorrectName()
    {
        var env = new StackEnvironment { Name = "test-env" };

        Assert.Equal("test-env", env.Name);
    }

    [Fact]
    public void StackEnvironment_AllPropertiesShouldBeSettable()
    {
        var env = new StackEnvironment
        {
            Name = "prod",
            Vault = "my-vault",
            Outpost = "my-outpost",
            CertIssuer = "lets-encrypt",
            RegistryCredentials = "my-creds",
            Repository = "https://github.com/test/repo",
            Remote = "my-remote",
            IsDeleted = true,
            Version = "environment.talaryon.io/v2"
        };

        Assert.Equal("prod", env.Name);
        Assert.Equal("my-vault", env.Vault);
        Assert.Equal("my-outpost", env.Outpost);
        Assert.Equal("lets-encrypt", env.CertIssuer);
        Assert.Equal("my-creds", env.RegistryCredentials);
        Assert.Equal("https://github.com/test/repo", env.Repository);
        Assert.Equal("my-remote", env.Remote);
        Assert.True(env.IsDeleted);
        Assert.Equal("environment.talaryon.io/v2", env.Version);
    }

    [Fact]
    public void StackEnvironmentBuilder_WithName_ShouldCreateEnvironment()
    {
        // Use a unique name that won't exist as a directory
        var uniqueName = Guid.NewGuid().ToString();
        var builder = new StackEnvironmentBuilder();
        var env = builder.WithName(uniqueName).Build();

        Assert.Equal(uniqueName, env.Name);
    }

    [Fact]
    public void StackEnvironmentBuilder_WithName_ShouldThrowIfDirectoryExists()
    {
        // Create a temp directory in current directory to simulate existing environment
        var originalDir = Environment.CurrentDirectory;
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Change to temp directory
            Environment.CurrentDirectory = tempDir;
            
            // Create a subdirectory that will match the environment name
            var envName = "existing-env";
            Directory.CreateDirectory(envName);
            
            var builder = new StackEnvironmentBuilder();
            
            // This should throw because the directory exists
            var ex = Assert.Throws<Talaryon.StackManager.Exceptions.EnvironmentAlreadyExistsException>(
                () => builder.WithName(envName).Build());
            
            Assert.Contains(envName, ex.Message);
        }
        finally
        {
            Environment.CurrentDirectory = originalDir;
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void StackEnvironmentBuilder_WithoutName_ShouldThrow()
    {
        var builder = new StackEnvironmentBuilder();
        
        var ex = Assert.Throws<ArgumentNullException>(() => builder.Build());
        Assert.Contains("name", ex.Message.ToLower());
    }

    [Fact]
    public void StackEnvironmentBuilder_Configure_ShouldApplyConfiguration()
    {
        // Use a unique name that won't exist as a directory
        var uniqueName = Guid.NewGuid().ToString();
        var builder = new StackEnvironmentBuilder();
        var env = builder
            .WithName(uniqueName)
            .Configure(e => 
            {
                e.Vault = "my-vault";
                e.Outpost = "my-outpost";
                e.CertIssuer = "lets-encrypt";
            })
            .Build();

        Assert.Equal(uniqueName, env.Name);
        Assert.Equal("my-vault", env.Vault);
        Assert.Equal("my-outpost", env.Outpost);
        Assert.Equal("lets-encrypt", env.CertIssuer);
    }
}
